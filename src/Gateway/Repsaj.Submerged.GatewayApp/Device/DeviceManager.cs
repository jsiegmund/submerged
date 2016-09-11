﻿using AutoMapper;
using Newtonsoft.Json.Linq;
using RemoteArduino.Commands;
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
using Repsaj.Submerged.GatewayApp.Modules;

namespace Repsaj.Submerged.GatewayApp.Device
{
    internal class DeviceManager : IDeviceManager
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

        ThreadPoolTimer _timer;
        ConnectionInformationModel _connectionInfo;
        DeviceModel _deviceModel;

        SynchronizationContext _synchronizationContext;

        int requestCounter = 0;

        public DeviceManager(ICommandProcessorFactory commandProcessor, IModuleConnectionManager moduleConnectionManager, 
            IConfigurationRepository configurationRepository, SynchronizationContext synchronizationContext)
        {
            _configurationRepository = configurationRepository;
            _synchronizationContext = synchronizationContext;
            _commandProcessorFactory = commandProcessor;

            _moduleConnectionManager = moduleConnectionManager;
            _moduleConnectionManager.ModulesInitialized += _moduleConnectionManager_ModulesInitialized;
            //_moduleConnectionManager.ModuleConnected += _arduinoConnectionManager_ModuleStatusChanged;
            //_moduleConnectionManager.ModuleDisconnected += _arduinoConnectionManager_ModuleStatusChanged;
            _moduleConnectionManager.ModuleStatusChanged += _moduleConnectionManager_ModuleStatusChanged;
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

            // initialize the Azure connection to test it for connectivity
            await InitializeAzure();

            _deviceModel = await _configurationRepository.GetDeviceModel();

            if (_deviceModel == null)
            {
                await RequestDeviceUpdate();
                NewLogLine?.Invoke("Devicemodel is missing, requesting from the cloud.");
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
            // TODO: remove any modules that might have been deleted
            var newModules = updatedModel.Modules.Where(m => !_deviceModel.Modules.Any(m2 => m.Name == m2.Name)).ToList();
            foreach (var newModule in newModules)
            {
                _deviceModel.Modules.Add(newModule);
            }

            // update the list of sensors and relays by clearing and re-adding 
            _deviceModel.Sensors.Clear();
            _deviceModel.Sensors.AddRange(updatedModel.Sensors);

            _deviceModel.Relays.Clear();
            _deviceModel.Relays.AddRange(updatedModel.Relays);

            // run the initialization of any new modules that might have been added
            if (newModules.Count() > 0)
                _moduleConnectionManager.InitializeModules(_deviceModel.Modules, _deviceModel.Sensors, _deviceModel.Relays);
        }

        #region Command Processor Callbacks
        private async void SwitchRelayProcessor_RelaySwitched(string relayName, bool relayState)
        {
            // update the relay 
            UpdateRelayData(relayName, relayState);

            // when successful; save the altered device model to store state
            await _configurationRepository.SaveDeviceModel(this._deviceModel);
        }

        private void DeviceInfoProcessor_DeviceModelChanged(DeviceModel deviceModel)
        {
            UpdateDeviceModel(deviceModel);
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
        private void UpdateSensorData(IEnumerable<SensorTelemetryModel> data)
        {
            List<string> processedSensors = new List<string>();

            foreach (var sensorData in data)
            {
                var sensorItem = _deviceModel.Sensors.SingleOrDefault(s => s.Name == sensorData.SensorName);

                if (sensorItem != null)
                {
                    sensorItem.Reading = sensorData.Value;
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

        private void UpdateModuleData(string moduleName, ModuleConnectionStatus moduleStatus)
        {
            var moduleItem = _deviceModel.Modules.SingleOrDefault(m => m.Name == moduleName);

            if (moduleItem != null)
            {
                moduleItem.Status = ModuleConnectionStatusAsText(moduleStatus);
                InvokeModulesUpdate();
            }
        }

        private void InvokeModulesUpdate()
        {
            ModulesUpdated?.Invoke(_deviceModel.Modules);
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
        private async Task InitializeAzure()
        {
            // Fetch the configuration information from the config file
            _connectionInfo = await _configurationRepository.GetConnectionInformationModel();

            // Create an AzureConnection class to connect to Azure
            _azureConnection = new AzureConnection(_connectionInfo.IoTHubHostname, _connectionInfo.DeviceId, _connectionInfo.DeviceKey);
            _azureConnection.CommandReceived += _azureConnection_CommandReceived;
            _azureConnection.Connected += _azureConnection_Connected;
            _azureConnection.Disconnected += _azureConnection_Disconnected;

            await _azureConnection.Init();
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

        private async Task SendDeviceData()
        {
            Dictionary<string, ModuleConnectionStatus> statuses = _moduleConnectionManager.GetModuleStatuses();
            Dictionary<string, string> statusesAsText = statuses.Select(s => new { s.Key, Value = ModuleConnectionStatusAsText(s.Value) })
                                                                .ToDictionary(s => s.Key, s => s.Value);

            dynamic data = DeviceFactory.GetDevice(_connectionInfo.DeviceId, _connectionInfo.DeviceKey, statusesAsText);

            try
            {
                // push the data to Azure for cloud processing
                await _azureConnection.SendDeviceToCloudMessageAsync(data);

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

            await _azureConnection.SendDeviceToCloudMessageAsync(requestObject);
        }
        #endregion

        #region Arduino / Module Connections
        private void RequestArduinoData()
        {
            var sensorData = _moduleConnectionManager.GetSensorData();
         
            if (sensorData?.Count() > 0)
            {
                UpdateSensorData(sensorData);

                // we want to send new telemetry data every 6 requests ( = once a minute)
                requestCounter = (requestCounter + 1) % 6;
                if (requestCounter == 0)
                {
                    Task.Run(() => SendTelemetryAsync(sensorData));
                }
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
                string message = await _azureConnection.SendDeviceToCloudMessageAsync(payload);

                NewLogLine?.Invoke(String.Format("Telemetry sent @ {0:G}", DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError($"Failure trying to send the data to Azure: {ex}");
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
            NewLogLine?.Invoke(String.Format("Received cloud 2 device message: {0} @ {1:G}", command.CommandName, DateTime.UtcNow));
            ICommandProcessor processor = _commandProcessorFactory.FindCommandProcessor(command);

            if (processor != null)
            {
                await processor.ProcessCommand(command);
                NewLogLine?.Invoke("Processed cloud 2 device message successfully.");
            }
        }

        private async void _moduleConnectionManager_ModulesInitialized()
        {
            NewLogLine?.Invoke("All modules have been intialized.");
            
            await SendDeviceData();
            StartTimer();
        }

        private void _moduleConnectionManager_ModuleStatusChanged(string moduleName, ModuleConnectionStatus newStatus)
        {
            UpdateModuleData(moduleName, newStatus);
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
