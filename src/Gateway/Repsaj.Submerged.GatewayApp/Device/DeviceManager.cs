using AutoMapper;
using Newtonsoft.Json.Linq;
using RemoteArduino.Commands;
using Repsaj.Submerged.GatewayApp.Arduino;
using Repsaj.Submerged.GatewayApp.Models;
using Repsaj.Submerged.GatewayApp.Universal.Device;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;

namespace Repsaj.Submerged.GatewayApp.Device
{
    internal class DeviceManager : IDeviceManager
    {
        public event Action<IEnumerable<Sensor>> SensorDataChanged;
        public event Action<IEnumerable<Module>> ModuleDataChanged;
        public event Action<IEnumerable<Relay>> RelayDataChanged;
        public event Action AzureConnected;
        public event Action AzureDisconnected;
        public event UpdateLog NewLogLine;

        ICommandProcessorFactory _commandProcessorFactory;
        IModuleConnectionManager _moduleConnectionManager;
        IConfigurationRepository _configurationRepository;
        IAzureConnection _azureConnection;

        ThreadPoolTimer _timer;
        ConnectionInformationModel _connectionInfo;
        DeviceModel _deviceModel;

        SynchronizationContext _synchronizationContext;

        int requestCounter = 0;

        public DeviceManager(ICommandProcessorFactory commandProcessor, IModuleConnectionManager moduleConnectionManager, 
            IConfigurationRepository configurationRepository, SynchronizationContext synchronizationContext)
        {
            _moduleConnectionManager = moduleConnectionManager;
            _configurationRepository = configurationRepository;
            _synchronizationContext = synchronizationContext;
            _commandProcessorFactory = commandProcessor;
        }

        public async Task Init()
        {
            NewLogLine?.Invoke("Performing initialization of the Gateway device, please wait.");

            // attach the device info updated event to a device info command processor (when found)
            DeviceInfoCommandProcessor deviceInfoProcessor = (DeviceInfoCommandProcessor)_commandProcessorFactory.FindCommandProcessor(CommandNames.UPDATE_INFO);
            deviceInfoProcessor.DeviceModelChanged += DeviceInfoProcessor_DeviceModelChanged;

            SwitchRelayCommandProcessor switchRelayProcessor = (SwitchRelayCommandProcessor)_commandProcessorFactory.FindCommandProcessor(CommandNames.SWITCH_RELAY);
            switchRelayProcessor.RelaySwitched += SwitchRelayProcessor_RelaySwitched;

            await _moduleConnectionManager.Init();

            Task azureTask = Task.Run(() => InitializeAzure());

            _deviceModel = await _configurationRepository.GetDeviceModel();

            if (_deviceModel == null)
            {
                // device should now request for the device model to be pushed from the back-end
                azureTask.Wait();
                await RequestDeviceUpdate();

                NewLogLine?.Invoke("Devicemodel is missing, requesting from the cloud.");
            }
            else
            {
                PopulateDeviceComponents();

                _moduleConnectionManager.ModulesInitialized += _arduinoConnectionManager_ModulesInitialized;
                _moduleConnectionManager.ModuleConnected += _arduinoConnectionManager_ModuleStatusChanged;
                _moduleConnectionManager.ModuleDisconnected += _arduinoConnectionManager_ModuleStatusChanged;
                _moduleConnectionManager.ModuleConnecting += _arduinoConnectionManager_ModuleStatusChanged;

                Task moduleTask = Task.Run(() => _moduleConnectionManager.InitializeModules(_deviceModel.Modules));

                Task.WaitAll(moduleTask, azureTask);
                NewLogLine?.Invoke("Device initialization completed!");
            }
        }

        public void Init(DeviceModel deviceModel)
        {
            _deviceModel = deviceModel;

            PopulateDeviceComponents();
            _moduleConnectionManager.InitializeModules(_deviceModel.Modules);
        }

        private void PopulateDeviceComponents()
        {
            SensorDataChanged?.Invoke(_deviceModel.Sensors.OrderBy(s => s.OrderNumber));
            //ModuleDataChanged?.Invoke(_deviceModel.Modules.OrderBy(s => s.DisplayOrder));
            RelayDataChanged?.Invoke(_deviceModel.Relays.OrderBy(s => s.OrderNumber));
        }

        #region Command Processor Callbacks
        private async void SwitchRelayProcessor_RelaySwitched(int relayNumber, bool relayState)
        {
            // update the relay 
            UpdateRelayData(relayNumber, relayState);

            // when successful; save the altered device model to store state
            await _configurationRepository.SaveDeviceModel(this._deviceModel);
        }

        private void DeviceInfoProcessor_DeviceModelChanged(DeviceModel deviceModel)
        {
            Init(deviceModel);
        }
        #endregion

        #region Timer
        private void StartTimer()
        {
            if (_timer == null)
            {
                // when all modules have initialized; create a new timer 
                // and kick off the timer event once manually to immediately send the latest data
                _timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, new TimeSpan(0, 0, 10));
                Timer_Tick(_timer);
            }
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Cancel();
                _timer = null;
            }
        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            RequestArduinoData();
        }
        #endregion

        #region Status Updates
        private void UpdateSensorData(JObject data)
        {
            List<string> processedSensors = new List<string>();

            foreach (var sensorData in data)
            {
                JToken valueToken = sensorData.Value;
                JValue value = (JValue)valueToken.Value<object>();

                var sensorItem = _deviceModel.Sensors.SingleOrDefault(s => s.Name == sensorData.Key);

                if (sensorItem != null)
                {
                    sensorItem.Reading = value.Value;
                    processedSensors.Add(sensorItem.Name);
                }
            }

            // if connections have been lost, sensor data might be missing
            // find the sensors for which there was no data processed and set the reading to null
            var missingSensors = _deviceModel.Sensors.Where(s => !processedSensors.Contains(s.Name));
            foreach (var sensor in missingSensors)
            {
                sensor.Reading = null;
            }

            SensorDataChanged?.Invoke(_deviceModel.Sensors.OrderBy(s => s.OrderNumber));
        }

        private void UpdateRelayData(int relayNumber, bool relayState)
        {
            Relay relay = _deviceModel.Relays.SingleOrDefault(r => r.RelayNumber == relayNumber);

            if (relay != null)
            {
                relay.State = relayState;
                RelayDataChanged?.Invoke(_deviceModel.Relays.OrderBy(r => r.OrderNumber));
            }
        }

        private void UpdateModuleData()
        {
            Dictionary<string, string> statuses = _moduleConnectionManager.GetModuleStatuses();

            foreach (KeyValuePair<string, string> kvp in statuses)
            {
                var moduleItem = _deviceModel.Modules.SingleOrDefault(m => m.Name == kvp.Key);

                if (moduleItem != null)
                    moduleItem.Status = kvp.Value;
            }

            ModuleDataChanged?.Invoke(_deviceModel.Modules.OrderBy(m => m.DisplayOrder));
        }
        #endregion

        #region Azure 
        private async Task InitializeAzure()
        {
            // Fetch the configuration information from the config file
            _connectionInfo = await _configurationRepository.GetConnectionInformationModel();

            // Create an AzureConnection class to connect to Azure
            _azureConnection = new AzureConnection(_connectionInfo.IoTHubHostname, _connectionInfo.DeviceId, _connectionInfo.DeviceKey);
            _azureConnection.CommandReceived += _azureConnection_CommandReceived;
            _azureConnection.Connected += _azureConnection_Connected;
            _azureConnection.Disconnected += _azureConnection_Disconnected;
            Debug.WriteLine("Azure connection initialized.");
        }

        private void _azureConnection_Disconnected()
        {
            this.AzureDisconnected?.Invoke();
        }

        private void _azureConnection_Connected()
        {
            this.AzureConnected?.Invoke();
        }

        private async Task SendTelemetryAsync(JObject data)
        {
            try
            {
                TelemetryHelper.MakeTelemetryObject(data, _connectionInfo.DeviceId);

                // push the data to Azure for cloud processing
                string message = await _azureConnection.SendDeviceToCloudMessagesAsync(data);

                NewLogLine?.Invoke(String.Format("Telemetry sent @ {0:G}", DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError($"Failure trying to send the data to Azure: {ex}");
                NewLogLine?.Invoke(String.Format("Failure trying to send the data to Azure."));
            }
        }

        private async Task SendDeviceData()
        {
            Dictionary<string, string> statuses = _moduleConnectionManager.GetModuleStatuses();
            dynamic data = DeviceFactory.GetDevice(_connectionInfo.DeviceId, _connectionInfo.DeviceKey, statuses);

            try
            {
                // push the data to Azure for cloud processing
                await _azureConnection.SendDeviceToCloudMessagesAsync(data);

                NewLogLine?.Invoke(String.Format("Device data sent @ {0:G}", DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError($"Failure trying to send device data to Azure: {ex}");
                NewLogLine?.Invoke(ex.ToString());
            }
        }

        public async Task RequestDeviceUpdate()
        {
            JObject requestObject = new JObject();
            requestObject.Add(DeviceModelConstants.OBJECT_TYPE, DeviceMessageObjectTypes.UPDATE_REQUEST);
            requestObject.Add(DevicePropertiesConstants.DEVICE_ID, _connectionInfo.DeviceId);

            await _azureConnection.SendDeviceToCloudMessagesAsync(requestObject);
        }    
        #endregion

        #region Arduino / Module Connections
        private void RequestArduinoData()
        {
            JObject data = _moduleConnectionManager.GetAvailableData();

            if (data != null)
            {
                JObject dataToBind = (JObject)data.DeepClone();
                UpdateSensorData(dataToBind);

                // we want to send new telemetry data every 6 requests ( = once a minute)
                requestCounter = (requestCounter + 1) % 6;
                if (requestCounter == 0)
                    Task.Run(() => SendTelemetryAsync(data));
            }
        }


        private async Task _azureConnection_CommandReceived(DeserializableCommand command)
        {
            NewLogLine?.Invoke(String.Format("Received cloud 2 device message: {0} @ {1:G}", command.CommandName, DateTime.UtcNow));
            ICommandProcessor processor = _commandProcessorFactory.FindCommandProcessor(command);

            if (processor != null)
            {
                await processor.ProcessCommand(command);
                NewLogLine?.Invoke("Processed cloud 2 device message successfully.");
            }
        }

        //private void _arduinoConnection_ConnectionFailed(string message)
        //{
        //    NewLogLine?.Invoke(message);
        //}

        private async void _arduinoConnectionManager_ModulesInitialized()
        {
            NewLogLine?.Invoke("All modules have been intialized.");

            UpdateModuleData();
            await SendDeviceData();
            StartTimer();
        }

        private void _arduinoConnectionManager_ModuleStatusChanged(string moduleName, bool notConnecting)
        {
            Task.Run(() => UpdateModuleData());
        }
        #endregion

        public void Dispose()
        {
            if (_moduleConnectionManager != null)
            {
                _moduleConnectionManager.Dispose();
                _moduleConnectionManager = null;
            }

            StopTimer();
        }
    }
}
