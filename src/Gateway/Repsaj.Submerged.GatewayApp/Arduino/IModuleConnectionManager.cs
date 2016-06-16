using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Models;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Arduino
{
    public delegate void IModuleStatusChanged(string moduleName, bool notConnecting);

    public interface IModuleConnectionManager : IDisposable
    {
        event Action ModulesInitialized;
        event IModuleStatusChanged ModuleConnecting;
        event IModuleStatusChanged ModuleConnected;
        event IModuleStatusChanged ModuleDisconnected;

        Dictionary<string, string> GetModuleStatuses();
        void InitializeModules(IEnumerable<Module> modules);
        JObject GetAvailableData();
        //IEnumerable<ModuleStatusModel> GetModuleStatusModels();

        bool AllModulesInitialized { get; }

        Task Init();
    }
}
