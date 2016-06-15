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

        ICommandProcessorFactory _commandProcessorFactory;
        IModuleConnectionManager _arduinoConnectionManager;
        IConfigurationRepository _configurationRepository;
        IAzureConnection _azureConnection;

        ThreadPoolTimer _timer;
        ConnectionInformationModel _connectionInfo;
        DeviceModel _deviceModel;

        SynchronizationContext _synchronizationContext;

        int requestCounter = 0;

        public event UpdateLog NewLogLine;
        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceManager(ICommandProcessorFactory commandProcessor, IModuleConnectionManager moduleConnectionManager, 
            IConfigurationRepository configurationRepository, SynchronizationContext synchronizationContext)
        {
            _commandProcessorFactory = commandProcessor;
            _arduinoConnectionManager = moduleConnectionManager;
            _configurationRepository = configurationRepository;
            _synchronizationContext = synchronizationContext;
        }

        public async Task Init()
        {
            await _arduinoConnectionManager.Init();

            _connectionInfo = await _configurationRepository.GetConnectionInformationModel();
            _deviceModel = await _configurationRepository.GetDeviceModel();

            PopulateDeviceComponents();

            _arduinoConnectionManager.ModulesInitialized += _arduinoConnectionManager_ModulesInitialized;
            _arduinoConnectionManager.ModuleConnected += _arduinoConnectionManager_ModuleStatusChanged;
            _arduinoConnectionManager.ModuleDisconnected += _arduinoConnectionManager_ModuleStatusChanged;
            _arduinoConnectionManager.ModuleConnecting += _arduinoConnectionManager_ModuleStatusChanged;

            Task moduleTask = Task.Run(() => _arduinoConnectionManager.ConnectModules());
            Task azureTask = Task.Run(() => InitializeAzure());

            Task.WaitAll(moduleTask, azureTask);
        }

        private void PopulateDeviceComponents()
        {
            SensorDataChanged?.Invoke(_deviceModel.Sensors);
            ModuleDataChanged?.Invoke(_deviceModel.Modules);
        }

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

        private void NotifyPropertyChanged(string info)
        {
            
        }

        #region Status Updates
        private void UpdateSensorData(JObject data)
        {
            foreach (var sensorData in data)
            {
                JToken valueToken = sensorData.Value;
                JValue value = (JValue)valueToken.Value<object>();

                var sensorItem = _deviceModel.Sensors.SingleOrDefault(s => s.Name == sensorData.Key);

                if (sensorItem != null)
                    sensorItem.Reading = value.Value;
                else
                    Debug.WriteLine($"Could not find a sensor named {sensorData.Key}");
            }

            SensorDataChanged?.Invoke(_deviceModel.Sensors);
        }

        private void UpdateRelayData()
        {

        }

        private void UpdateModuleData()
        {
            Dictionary<string, string> statuses = _arduinoConnectionManager.GetModuleStatuses();

            foreach (KeyValuePair<string, string> kvp in statuses)
            {
                var moduleItem = _deviceModel.Modules.SingleOrDefault(m => m.Name == kvp.Key);

                if (moduleItem != null)
                    moduleItem.Status = kvp.Value;
                else
                    Debug.WriteLine($"Could not find a module named {kvp.Key}");
            }

            ModuleDataChanged?.Invoke(_deviceModel.Modules);
        }
        #endregion


        #region Azure 
        private void InitializeAzure()
        {
            // Create an AzureConnection class to connect to Azure
            _azureConnection = new AzureConnection(_connectionInfo.IoTHubHostname, _connectionInfo.DeviceId, _connectionInfo.DeviceKey);
            _azureConnection.CommandReceived += _azureConnection_CommandReceived;
            Debug.WriteLine("Azure connection initialized.");
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
            Dictionary<string, string> statuses = _arduinoConnectionManager.GetModuleStatuses();
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
        #endregion

        #region Arduino / Module Connections
        private void RequestArduinoData()
        {
            JObject data = _arduinoConnectionManager.GetAvailableData();

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
            NewLogLine?.Invoke("Received cloud 2 device message: " + command);
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
            if (_arduinoConnectionManager != null)
            {
                _arduinoConnectionManager.Dispose();
                _arduinoConnectionManager = null;
            }

            StopTimer();
        }
    }
}
