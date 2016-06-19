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

namespace Repsaj.Submerged.GatewayApp.Arduino
{
    public class ModuleConnectionManager : IModuleConnectionManager
    {
        IModuleConnectionFactory _moduleConnectionFactory;

        IEnumerable<Module> _modules;
        Dictionary<string, ModuleConnection> _moduleConnections = new Dictionary<string, ModuleConnection>();
        Dictionary<string, ModuleConnectionStatus> _moduleStatuses = new Dictionary<string, ModuleConnectionStatus>();

        public event IModuleStatusChanged ModuleConnecting;
        public event IModuleStatusChanged ModuleConnected;
        public event IModuleStatusChanged ModuleDisconnected;
        public event Action ModulesInitialized;

        public bool AllModulesInitialized { get; private set; }


        public ModuleConnectionManager(IModuleConnectionFactory moduleConnectionFactory)
        {
            _moduleConnectionFactory = moduleConnectionFactory;
            AllModulesInitialized = false;
        }

        public async Task Init()
        {
            await _moduleConnectionFactory.Init();
        }

        public void InitializeModules(IEnumerable<Module> modules)
        {
            _modules = modules;

            // Initialize all of the Arduino connections
            foreach (Module module in modules)
            {
                // skip the module when we already have it
                if (_moduleConnections.ContainsKey(module.Name))
                    continue;

                try
                {
                    //MinimalEventSource.Log.LogInfo($"Requesting module {module.Name} from factory. Bluetooth id: {module.ConnectionString}");

                    ModuleConnection connection = _moduleConnectionFactory.GetModuleConnection(module);

                    if (connection != null)
                    {
                        connection.ModuleStatusChanged += Connection_ModuleStatusChanged;

                        _moduleConnections.Add(module.Name, connection);
                        _moduleStatuses[connection.ModuleName] = connection.ModuleStatus;
                    }
                }
                catch (Exception ex)
                {
                    //MinimalEventSource.Log.LogError($"Could not initialize a module connection to module {module.Name} because: {ex}");
                }
            }
        }

        private void Connection_ModuleStatusChanged(string moduleName, ModuleConnectionStatus oldStatus, ModuleConnectionStatus newStatus)
        {
            // connectionToggle is only set true when the module switches from connected => disconnected and vice versa
            // we ignore "connecting" because that would cause a lot of chatter with Azure since a disconnected 
            // module will be connecting every x minutes
            bool connectionToggle = false;
            if (newStatus != ModuleConnectionStatus.Connecting && _moduleStatuses[moduleName] != newStatus)
            {
                _moduleStatuses[moduleName] = newStatus;
                connectionToggle = true;
            }

            switch (newStatus)
            {
                case ModuleConnectionStatus.Connecting:
                    ModuleConnecting?.Invoke(moduleName, connectionToggle);
                    break;
                case ModuleConnectionStatus.Disconnected:
                    ModuleDisconnected?.Invoke(moduleName, connectionToggle);
                    break;
                case ModuleConnectionStatus.Connected:
                    ModuleConnected?.Invoke(moduleName, connectionToggle);
                    break;
            }

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
                KeyValuePair<string, ModuleConnection> moduleKvp = _moduleConnections.First();
                moduleKvp.Value.Dispose();
                _moduleConnections.Remove(moduleKvp.Key);
            }
        }

        public JObject GetAvailableData()
        {
            JObject data = new JObject();

            bool dataPresent = false;
            IEnumerable<ModuleConnection> connectedModules = _moduleConnections.Values.Where(m => m.ModuleStatus == ModuleConnectionStatus.Connected).ToArray();

            // loop all of the available modules, request their data and merge it into our data object
            foreach (var module in connectedModules)
            {
                try
                {
                    dynamic moduleData = module.RequestArduinoData();
                    TelemetryHelper.Merge(data, moduleData);
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

        public Dictionary<string, string> GetModuleStatuses()
        {
            Dictionary<string, string> statuses = new Dictionary<string, string>();

            foreach (var module in _modules)
            {
                if (_moduleConnections.ContainsKey(module.Name))
                    statuses.Add(module.Name, _moduleConnections[module.Name].StatusAsText);
                else
                    statuses.Add(module.Name, "Error");
            }

            return statuses;
        }
    }
}
