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
using Repsaj.Submerged.GatewayApp.Models;

namespace Repsaj.Submerged.GatewayApp.Modules
{
    public class ModuleConnectionManager : IModuleConnectionManager
    {
        IModuleConnectionFactory _moduleConnectionFactory;

        IEnumerable<Module> _modules;
        Dictionary<string, IModuleConnection> _moduleConnections = new Dictionary<string, IModuleConnection>();
        Dictionary<string, ModuleConnectionStatus> _moduleStatuses = new Dictionary<string, ModuleConnectionStatus>();

        public event ModuleStatusChanged ModuleStatusChanged;
        public event Action ModulesInitialized;

        public bool AllModulesInitialized { get; private set; }
        private static object _moduleLock = new object();


        public ModuleConnectionManager(IModuleConnectionFactory moduleConnectionFactory)
        {
            _moduleConnectionFactory = moduleConnectionFactory;
            AllModulesInitialized = false;
        }

        public async Task Init()
        {
            await _moduleConnectionFactory.Init();
        }

        public void InitializeModules(IEnumerable<Module> modules, IEnumerable<Sensor> sensors, IEnumerable<Relay> relays)
        {
            _modules = modules;

            lock (_moduleLock)
            {
                // Initialize all of the Arduino connections
                foreach (Module module in modules)
                {
                    // skip the module when we already have it
                    if (_moduleConnections.ContainsKey(module.Name))
                        continue;

                    // if there's a new module to initialize; set the 'all initialized' flag to false again
                    AllModulesInitialized = false;

                    try
                    {
                        //MinimalEventSource.Log.LogInfo($"Requesting module {module.Name} from factory. Bluetooth id: {module.ConnectionString}");
                        Sensor[] moduleSensors = sensors.Where(s => s.Module == module.Name).ToArray();
                        Relay[] moduleRelays = relays.Where(r => r.Module == module.Name).ToArray();

                        IModuleConnection connection = _moduleConnectionFactory.GetModuleConnection(module, moduleSensors, moduleRelays);

                        _moduleConnections.Add(module.Name, connection);
                        _moduleStatuses[connection.ModuleName] = connection.ModuleStatus;
                        Connection_ModuleStatusChanged(module.Name, ModuleConnectionStatus.Disconnected, connection.ModuleStatus);

                        connection.ModuleStatusChanged += Connection_ModuleStatusChanged;
                    }
                    catch (Exception ex)
                    {
                        //MinimalEventSource.Log.LogError($"Could not initialize a module connection to module {module.Name} because: {ex}");
                    }
                }
            }
        }
        

        private void Connection_ModuleStatusChanged(string moduleName, ModuleConnectionStatus oldStatus, ModuleConnectionStatus newStatus)
        {
            // connectionToggle is only set true when the module switches from connected => disconnected and vice versa
            // we ignore "connecting" because that would cause a lot of chatter with Azure since a disconnected 
            // module will be connecting every x minutes
            //bool connectionToggle = false;

            if (newStatus != ModuleConnectionStatus.Connecting && _moduleStatuses[moduleName] != newStatus)
            {
                _moduleStatuses[moduleName] = newStatus;
                //connectionToggle = true;
            }

            //switch (newStatus)
            //{
            //    case ModuleConnectionStatus.Connecting:
            //        ModuleStatusChanged?.Invoke(moduleName, moduleConnection.StatusAsText);
            //        break;
            //    case ModuleConnectionStatus.Disconnected:
            //        ModuleDisconnected?.Invoke(moduleName, moduleConnection.StatusAsText);
            //        break;
            //    case ModuleConnectionStatus.Connected:
            //        ModuleConnected?.Invoke(moduleName, moduleConnection.StatusAsText);
            //        break;
            //}

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
        }

        public void Dispose()
        {
            while (_moduleConnections.Count > 0)
            {
                KeyValuePair<string, IModuleConnection> moduleKvp = _moduleConnections.First();
                moduleKvp.Value.Dispose();
                _moduleConnections.Remove(moduleKvp.Key);
            }
        }

        public JObject GetAvailableData()
        {
            JObject data = new JObject();

            bool dataPresent = false;
            IEnumerable<IModuleConnection> connectedModules = _moduleConnections.Values.Where(m => m.ModuleStatus == ModuleConnectionStatus.Connected).ToArray();

            // loop all of the available modules, request their data and merge it into our data object
            foreach (var module in connectedModules)
            {
                try
                {
                    JObject moduleData = module.RequestArduinoData();
                    data.Merge(moduleData);
                    dataPresent = true;
                }
                catch (Exception ex)
                {
                    //MinimalEventSource.Log.LogError($"Failure trying to get data from module {module.ModuleName}: {ex}");
                }
            }

            if (dataPresent)
                return data;
            else
                return null;
        }

        public ModuleConnectionStatus GetModuleStatus(string moduleName)
        {
            return _moduleConnections[moduleName].ModuleStatus;
        }

        public Dictionary<string, ModuleConnectionStatus> GetModuleStatuses()
        {
            Dictionary<string, ModuleConnectionStatus> statuses = new Dictionary<string, ModuleConnectionStatus>();

            lock (_moduleLock)
            {
                return _moduleConnections.Select(conn => new { conn.Key, conn.Value.ModuleStatus })
                                         .ToDictionary(c => c.Key, c => c.ModuleStatus);
            }
        }
    }
}
