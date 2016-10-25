using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Device;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Extensions;
using Repsaj.Submerged.Gateway.Common.Log;
using System.Threading;
using System.Collections.Concurrent;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
{
    public class ModuleConnectionManager : IModuleConnectionManager
    {
        IModuleConnectionFactory _moduleConnectionFactory;
        ISensorDataStore _sensorDatastore;
        
        ConcurrentDictionary<string, IModuleConnection> _moduleConnections = new ConcurrentDictionary<string, IModuleConnection>();
        ConcurrentDictionary<string, ModuleConnectionStatus> _moduleStatuses = new ConcurrentDictionary<string, ModuleConnectionStatus>();

        public event ModuleStatusChanged ModuleStatusChanged;
        public event Action ModulesInitialized;

        public bool AllModulesInitialized { get; private set; }

        public ModuleConnectionManager(IModuleConnectionFactory moduleConnectionFactory, ISensorDataStore sensorDatastore)
        {
            _moduleConnectionFactory = moduleConnectionFactory;
            _sensorDatastore = sensorDatastore;

            AllModulesInitialized = false;
        }

        public async Task Init()
        {
            await _moduleConnectionFactory.Init();
        }

        public void InitializeModules(IEnumerable<Module> modules, IEnumerable<Sensor> sensors, IEnumerable<Relay> relays)
        {
            // Initialize all of the Arduino connections
            foreach (Module module in modules)
            {
                // skip the module when we already have it
                if (_moduleConnections.ContainsKey(module.Name))
                {
                    var existingConnection = _moduleConnections[module.Name];
                    existingConnection.UpdateConfiguration(module.Configuration);

                    continue;
                }

                // if there's a new module to initialize; set the 'all initialized' flag to false again
                AllModulesInitialized = false;

                try
                {
                    LogEventSource.Log.Info($"Requesting module {module.Name} from factory. Bluetooth id: {module.ConnectionString}");

                    Sensor[] moduleSensors = sensors.Where(s => s.Module == module.Name).ToArray();
                    Relay[] moduleRelays = relays.Where(r => r.Module == module.Name).ToArray();

                    IModuleConnection connection = _moduleConnectionFactory.GetModuleConnection(module, moduleSensors, moduleRelays);

                    _moduleConnections.TryAdd(module.Name, connection);
                    _moduleStatuses[connection.ModuleName] = connection.ModuleStatus;
                    Connection_ModuleStatusChanged(module.Name, ModuleConnectionStatus.Disconnected, connection.ModuleStatus);

                    connection.ModuleStatusChanged += Connection_ModuleStatusChanged;
                }
                catch (Exception ex)
                {
                    LogEventSource.Log.Error($"Exception during initialization of modules in connection manager: {ex}");
                }
            }

            // modules that have been deleted from the model have to be disconnected and removed 
            var deletedModuleNames = _moduleConnections.Keys.Where(k => !modules.Any(m => m.Name == k));
            foreach (string name in deletedModuleNames)
            {
                IModuleConnection deletedModule = _moduleConnections[name];
                deletedModule.Dispose();
                _moduleConnections.TryRemove(name, out deletedModule);
            }
        }
        

        private async void Connection_ModuleStatusChanged(string moduleName, ModuleConnectionStatus oldStatus, ModuleConnectionStatus newStatus)
        {
            // connectionToggle is only set true when the module switches from connected => disconnected and vice versa
            // we ignore "connecting" because that would cause a lot of chatter with Azure since a disconnected 
            // module will be connecting every x minutes
            //bool connectionToggle = false;
            Debug.WriteLine($"ModuleStatusChanged was invoked for module {moduleName}, switching from  {oldStatus} to {newStatus}.");
            if (newStatus != ModuleConnectionStatus.Connecting && _moduleStatuses[moduleName] != newStatus)
            {
                _moduleStatuses[moduleName] = newStatus;
            }

            IModuleConnection moduleConnection = _moduleConnections[moduleName];

            if (oldStatus != newStatus)
                ModuleStatusChanged?.Invoke(moduleName, moduleConnection.ModuleStatus);

            if (oldStatus == ModuleConnectionStatus.Initializing)
            {
                // check to see whether there are any modules left initializing
                if (AllModulesInitialized == false &&
                    !_moduleConnections.Any(s => s.Value.ModuleStatus == ModuleConnectionStatus.Initializing))
                {
                    AllModulesInitialized = true;
                    ModulesInitialized?.Invoke();
                }
            }

            // automatically try to reconnect disconnected modules in 5 minutes time
            if (newStatus == ModuleConnectionStatus.Disconnected)
            {
                await ReconnectModule(moduleConnection);
            }
        }

        private async Task ReconnectModule(IModuleConnection moduleConnection)
        {
            Debug.WriteLine($"Delaying 5 minutes before reconnecting module {moduleConnection.ModuleName}");
            await Task.Delay(new TimeSpan(0, 5, 0));
            Debug.WriteLine($"Calling reconnect on module {moduleConnection.ModuleName}");
            await moduleConnection.Reconnect();
        }

        public void Dispose()
        {
            // dispose all of the registered modules so that they can properly close
            // their connections
            while (_moduleConnections.Count > 0)
            {
                IModuleConnection connection;

                KeyValuePair<string, IModuleConnection> moduleKvp = _moduleConnections.First();
                // TODO: Dispose should be changed to Disconnect
                moduleKvp.Value.Dispose();
                _moduleConnections.TryRemove(moduleKvp.Key, out connection);
            }
        }

        public async Task<IEnumerable<SensorTelemetryModel>> GetSensorData()
        {
            try
            {
                var type = typeof(ISensorModule);
                var connectedModules = _moduleConnections.Values.Where(m => m.ModuleStatus == ModuleConnectionStatus.Connected &&
                                                                            m is ISensorModule).Cast<ISensorModule>();

                // loop all of the available modules, request their data and merge it into our data object
                foreach (var module in connectedModules)
                {
                    IEnumerable<SensorTelemetryModel> moduleData = null;

                    try
                    {
                        moduleData = await module.RequestSensorData().TimeoutAfter(new TimeSpan(0, 0, 30));
                    }
                    catch (Exception ex)
                    {
                        LogEventSource.Log.Error($"Failure requesting sensor data from module {module.ModuleName}: {ex}");
                    }

                    // store all of the data in the data store; even when it's null
                    _sensorDatastore.ProcessData(module, moduleData);
                }
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error($"ModuleConnectionManager encountered an error trying to get sensor data: {ex}");
            }

            // return the data from the datastore which will automatically provide averages and factor out not functioning hardware
            return _sensorDatastore.GetData();
        }

        public ModuleConnectionStatus GetModuleStatus(string moduleName)
        {
            return _moduleConnections[moduleName].ModuleStatus;
        }

        public Dictionary<string, ModuleConnectionStatus> GetModuleStatuses()
        {
            Dictionary<string, ModuleConnectionStatus> statuses = new Dictionary<string, ModuleConnectionStatus>();

            return _moduleConnections.Select(conn => new { conn.Key, conn.Value.ModuleStatus })
                                     .ToDictionary(c => c.Key, c => c.ModuleStatus);
        }
    }
}
