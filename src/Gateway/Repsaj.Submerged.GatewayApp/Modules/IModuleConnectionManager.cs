using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Models;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Modules
{
    public delegate void ModuleStatusChanged(string moduleName, ModuleConnectionStatus newStatus);

    public interface IModuleConnectionManager : IDisposable
    {
        event Action ModulesInitialized;
        event ModuleStatusChanged ModuleStatusChanged;
        //event ModuleStatusChanged ModuleConnected;
        //event ModuleStatusChanged ModuleDisconnected;

        ModuleConnectionStatus GetModuleStatus(string moduleName);
        Dictionary<string, ModuleConnectionStatus> GetModuleStatuses();
        void InitializeModules(IEnumerable<Module> modules, IEnumerable<Sensor> sensors, IEnumerable<Relay> relays);
        IEnumerable<SensorTelemetryModel> GetSensorData();
        //IEnumerable<ModuleStatusModel> GetModuleStatusModels();

        bool AllModulesInitialized { get; }

        Task Init();
    }
}
