using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Device;
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
using Repsaj.Submerged.GatewayApp.Universal.Modules;
using Repsaj.Submerged.Gateway.Common.Log;
using Repsaj.Submerged.GatewayApp.Universal.Commands;
using Repsaj.Submerged.GatewayApp.Universal.Azure;

namespace Repsaj.Submerged.GatewayApp.Device
{
    public class DeviceManager : IDeviceManager
    {
        public event Action<IEnumerable<Module>> ModulesUpdated;
        public event Action<IEnumerable<Sensor>> SensorsUpdated;
        public event Action<IEnumerable<Relay>> RelaysUpdated;

        public event Action AzureConnected;
        public event Action AzureDisconnected;
        public event UpdateLog NewLogLine;

        ICommandProcessorFactory _commandProcessorFactory;
        IModuleConnectionManager _moduleConnectionManager;
        IConfigurationRepository _configurationRepository;
        IAzureConnection _azureConnection;

        ConnectionInformationModel _connectionInfo;
        DeviceModel _deviceModel;

        ThreadPoolTimer _timer;
        TimeSpan _dataInterval = new TimeSpan(0, 0, 10);
        int _sendDataEveryN = 6;
        int requestCounter = 0;

        public DeviceManager(ICommandProcessorFactory commandProcessor, IModuleConnectionManager moduleConnectionManager, 
            IConfigurationRepository configurationRepository, SynchronizationContext synchronizationContext, IAzureConnection azureConnection)
        {
            _configurationRepository = configurationRepository;
            _commandProcessorFactory = commandProcessor;
            _azureConnection = azureConnection;

            _moduleConnectionManager = moduleConnectionManager;
            _moduleConnectionManager.ModulesInitialized += _moduleConnectionManager_ModulesInitialized;
            _moduleConnectionManager.ModuleStatusChanged += _moduleConnectionManager_ModuleStatusChanged;

            _azureConnection.CommandReceived += _azureConnection_CommandReceived;
            _azureConnection.Connected += _azureConnection_Connected;
            _azureConnection.Disconnected += _azureConnection_Disconnected;

        }

        public async Task Init()
        {
            NewLogLine?.Invoke("Performing initialization of the Gateway device, please wait.");

            // attach the device info updated event to a device info command processor (when found)
            DeviceInfoCommandProcessor deviceInfoProcessor = (DeviceInfoCommandProcessor)_commandProcessorFactory.FindCommandProcessor(CommandNames.UPDATE_INFO);
            deviceInfoProcessor.DeviceModelChanged += DeviceInfoProcessor_DeviceModelChanged;

            SwitchRelayCommandProcessor switchRelayProcessor = (SwitchRelayCommandProcessor)_commandProcessorFactory.FindCommandProcessor(CommandNames.SWITCH_RELAY);
            switchRelayProcessor.RelaySwitched += SwitchRelayProcessor_RelaySwitched;

            // initialize the Azure connection to test it for connectivity
            _connectionInfo = await _configurationRepository.GetConnectionInformationModelAsync();
            await _azureConnection.Init(_connectionInfo);

            // initialize the module connection manager
            await _moduleConnectionManager.Init();

            _deviceModel = await _configurationRepository.GetDeviceModelAsync();

            if (_deviceModel == null)
            {
                await RequestDeviceUpdate();
                NewLogLine?.Invoke("Didn't find any information about this device, sent a request to the back-end to aqcuire it. It might take some time to get here, hold on.");
            }
            else
            {
                CleanLoadedDeviceModel();

                _moduleConnectionManager.InitializeModules(_deviceModel.Modules, _deviceModel.Sensors, _deviceModel.Relays);
                NewLogLine?.Invoke("Device initialization completed!");

                // always send a device update request to ensure we're running with the latest values
                await RequestDeviceUpdate();
            }
        }

        private void CleanLoadedDeviceModel()
        {
            foreach (var module in this._deviceModel.Modules)
                module.Status = string.Empty;

            foreach (var sensor in this._deviceModel.Sensors)
                sensor.Reading = null;
        }

        public void UpdateDeviceModel(DeviceModel updatedModel)
        {
            // if there's no device model yet; create a new empty one and copy
            // the properties of the updated device model to it
            // copying of sensors, relays and modules is done automatically by the update routines after this
            if (_deviceModel == null)
            {
                _deviceModel = new DeviceModel();
                _deviceModel.DeviceProperties = updatedModel.DeviceProperties;
            }

            // add any modules that might not yet exist in the current model
            // note; the ToList is there because newModules is otherwise evaluated as Count == 0 below
            var newModules = updatedModel.Modules.Where(m => !_deviceModel.Modules.Any(m2 => m.Name == m2.Name)).ToList();
            foreach (var newModule in newModules)
            {
                _deviceModel.Modules.Add(newModule);
            }

            // Remove modules that are no longer in the updated model
            var deletedModules = _deviceModel.Modules.Where(m => !updatedModel.Modules.Any(m2 => m.Name == m2.Name)).ToList();
            foreach (var deletedModule in deletedModules)
            {
                _deviceModel.Modules.Remove(deletedModule);
            }

            // update the list of sensors and relays by clearing and re-adding 
            _deviceModel.Sensors.Clear();
            _deviceModel.Sensors.AddRange(updatedModel.Sensors);

            _deviceModel.Relays.Clear();
            _deviceModel.Relays.AddRange(updatedModel.Relays);

            // Run the module initialization again. This will add new modules, update existing 
            // ones and deleted the ones that have been removed from the model.
            _moduleConnectionManager.InitializeModules(_deviceModel.Modules, _deviceModel.Sensors, _deviceModel.Relays);
        }

        #region Command Processor Callbacks
        private async void SwitchRelayProcessor_RelaySwitched(string relayName, bool relayState)
        {
            // update the relay 
            UpdateRelayData(relayName, relayState);

            // when successful; save the altered device model to store state
            await _configurationRepository.SaveDeviceModelAsync(this._deviceModel);
        }

        private void DeviceInfoProcessor_DeviceModelChanged(DeviceModel deviceModel)
        {
            UpdateDeviceModel(deviceModel);
        }
        #endregion

        #region Timer
        private void StartTimer()
        {
            Debug.WriteLine("Starting timer for sending device updates to the cloud.");

            if (_timer == null)
            {
                // when all modules have initialized; create a new timer 
                // and kick off the timer event once manually to immediately send the latest data
                _timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, _dataInterval);

                // kick off the request manually first time
                Task.Run(() => RequestModuleData());
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

        private async void Timer_Tick(ThreadPoolTimer timer)
        {
            await RequestModuleData();
        }
        #endregion

        #region Status Updates
        private void UpdateSensorData(IEnumerable<SensorTelemetryModel> data)
        {
            List<string> processedSensors = new List<string>();

            foreach (var sensorData in data)
            {
                var sensorItem = _deviceModel.Sensors.SingleOrDefault(s => s.Name == sensorData.SensorName);

                if (sensorItem != null)
                {
                    sensorItem.Reading = sensorData.Value;
                    sensorItem.Trend = Enum.GetName(typeof(TelemetryTrendIndication), sensorData.TrendIndication);
                    processedSensors.Add(sensorItem.Name);
                }
            }

            // if connections have been lost, sensor data might be missing
            // find the sensors for which there was no data processed and set the reading to null
            var missingSensors = _deviceModel.Sensors.Where(s => !processedSensors.Contains(s.Name));
            foreach (var sensor in missingSensors)
            {
                sensor.Reading = null;
                sensor.Trend = null;
            }

            InvokeSensorsUpdate();
        }

        private void InvokeSensorsUpdate()
        {
            var connectedSensorData = _deviceModel.Sensors
                                                  .Where(r => _moduleConnectionManager.GetModuleStatus(r.Module) == ModuleConnectionStatus.Connected);
            SensorsUpdated?.Invoke(connectedSensorData);
        }

        private void UpdateRelayData(string relayName, bool relayState)
        {
            Relay relay = _deviceModel.Relays.SingleOrDefault(r => r.Name == relayName);

            if (relay != null)
            {
                relay.State = relayState;
                InvokeRelaysUpdate();
            }
        }

        private void InvokeRelaysUpdate()
        {
            var relayData = _deviceModel.Relays
                            .Where(r => _moduleConnectionManager.GetModuleStatus(r.Module) == ModuleConnectionStatus.Connected);
            RelaysUpdated?.Invoke(relayData);
        }

        private async Task UpdateModuleData(string moduleName, ModuleConnectionStatus moduleStatus)
        {
            var moduleItem = _deviceModel.Modules.SingleOrDefault(m => m.Name == moduleName);

            if (moduleItem != null)
            {
                moduleItem.Status = ModuleConnectionStatusAsText(moduleStatus);
                await InvokeModulesUpdate();
            }
        }

        private async Task InvokeModulesUpdate()
        {
            ModulesUpdated?.Invoke(_deviceModel.Modules);

            // when everything was initialized before; send any module update
            // to the cloud
            if (_moduleConnectionManager.AllModulesInitialized)
                await SendDeviceData();
        }

        private string ModuleConnectionStatusAsText(ModuleConnectionStatus moduleStatus)
        {
            string moduleStatusAsText;

            // TODO; needs to move to a helper class 
            switch (moduleStatus)
            {
                case ModuleConnectionStatus.Connected:
                    moduleStatusAsText = "Connected";
                    break;
                case ModuleConnectionStatus.Connecting:
                    moduleStatusAsText = "Connecting";
                    break;
                case ModuleConnectionStatus.Disconnected:
                    moduleStatusAsText = "Disconnected";
                    break;
                case ModuleConnectionStatus.Initializing:
                    moduleStatusAsText = "Initializing";
                    break;
                case ModuleConnectionStatus.NotRegistered:
                    moduleStatusAsText = "Not registered";
                    break;
                default:
                    moduleStatusAsText = "Unknown";
                    break;
            }

            return moduleStatusAsText;
        }
        #endregion

        #region Azure 
        private void _azureConnection_Disconnected()
        {
            this.AzureDisconnected?.Invoke();
        }

        private void _azureConnection_Connected()
        {
            this.AzureConnected?.Invoke();
        }

        private async Task SendDeviceData()
        {
            Dictionary<string, ModuleConnectionStatus> statuses = _moduleConnectionManager.GetModuleStatuses();
            Dictionary<string, string> statusesAsText = statuses.Select(s => new { s.Key, Value = ModuleConnectionStatusAsText(s.Value) })
                                                                .ToDictionary(s => s.Key, s => s.Value);

            dynamic data = DeviceFactory.GetDevice(_connectionInfo.DeviceId, _connectionInfo.DeviceKey, statusesAsText);

            if (data == null)
            {
                LogEventSource.Log.Error($"DeviceFactory returned a null object for device {_connectionInfo.DeviceId}.");
                return;
            }

            // push the data to Azure for cloud processing
            bool success = await _azureConnection.SendDeviceToCloudMessageAsync(data);

            if (success)
                NewLogLine?.Invoke("Sent a status update to back-end.");
            else
                NewLogLine?.Invoke("The device could not send device data.");
        }

        public async Task RequestDeviceUpdate()
        {
            JObject requestObject = new JObject();
            requestObject.Add(DeviceModelConstants.OBJECT_TYPE, DeviceMessageObjectTypes.UPDATE_REQUEST);
            requestObject.Add(DevicePropertiesConstants.DEVICE_ID, _connectionInfo.DeviceId);

            bool success = await _azureConnection.SendDeviceToCloudMessageAsync(requestObject);
        }
        #endregion

        #region Arduino / Module Connections
        private async Task RequestModuleData()
        {
            try
            {
                // The module connection manager already returns running averages of sensor values
                var sensorData = await _moduleConnectionManager.GetSensorData();

                if (sensorData?.Count() > 0)
                {
                    // Update the sensor data in the UI after each new data packet
                    UpdateSensorData(sensorData);

                    // the data is sent out to the cloud every N times after fetching it
                    requestCounter = ++requestCounter % _sendDataEveryN;
                    if (requestCounter == 0)
                    {
                        await SendTelemetryAsync(sensorData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error($"Something went wrong requesting module data: {ex}");
            }
        }

        private async Task SendTelemetryAsync(IEnumerable<SensorTelemetryModel> sensorData)
        { 
            try
            {
                sensorData = PrepareSensorData(sensorData);

                DeviceTelemetryModel telemetryModel = new DeviceTelemetryModel()
                {
                    DeviceId = _connectionInfo.DeviceId,
                    SensorData = sensorData
                };

                string payload = Newtonsoft.Json.JsonConvert.SerializeObject(telemetryModel);

                // push the data to Azure for cloud processing
                bool success = await _azureConnection.SendDeviceToCloudMessageAsync(payload);

                if (success)
                    NewLogLine?.Invoke(String.Format("Telemetry sent @ {0:G}", DateTime.Now));
                else
                    NewLogLine?.Invoke(String.Format("Could not send telemetry, the connection is down.", DateTime.Now));
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error($"Failure trying to send the data to Azure: {ex}");
                NewLogLine?.Invoke(String.Format("Failure trying to send the data to Azure."));
            }
        }

        private IEnumerable<SensorTelemetryModel> PrepareSensorData(IEnumerable<SensorTelemetryModel> sensorData)
        {
            List<SensorTelemetryModel> preppedData = new List<SensorTelemetryModel>(sensorData);

            // remove all sensor floats that not high, no relevance in storing that
            var stockFloatSensors = _deviceModel.Sensors.Where(s => s.SensorType == SensorTypes.STOCKFLOAT);
            preppedData.RemoveAll(d => stockFloatSensors.Any(sf => sf.Name == d.SensorName) && (bool?)d.Value == false);

            // remove all moisture sensors that read false
            var moistureSensors = _deviceModel.Sensors.Where(s => s.SensorType == SensorTypes.MOISTURE);
            preppedData.RemoveAll(d => moistureSensors.Any(ms => ms.Name == d.SensorName) && (bool?)d.Value == false);

            return preppedData;
        }

        private async Task _azureConnection_CommandReceived(DeserializableCommand command)
        {
            NewLogLine?.Invoke(String.Format("Received cloud 2 device message: {0} @ {1:G}", command.CommandName, DateTime.Now));
            ICommandProcessor processor = _commandProcessorFactory.FindCommandProcessor(command);

            if (processor != null)
            {
                await processor.ProcessCommand(command);
                NewLogLine?.Invoke("Processed cloud 2 device message successfully.");
            }
        }

        private async void _moduleConnectionManager_ModulesInitialized()
        {
            NewLogLine?.Invoke("All modules have intialized.");
            
            await SendDeviceData();
            StartTimer();
        }

        private async void _moduleConnectionManager_ModuleStatusChanged(string moduleName, ModuleConnectionStatus newStatus)
        {
            await UpdateModuleData(moduleName, newStatus);
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
