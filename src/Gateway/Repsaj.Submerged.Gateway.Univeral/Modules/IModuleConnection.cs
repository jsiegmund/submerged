using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
{
    public delegate void IModuleConnectionStatusChanged(string moduleName, ModuleConnectionStatus oldStatus, ModuleConnectionStatus newStatus);

    public interface IModuleConnection : IDisposable
    {
        string ModuleName { get; }
        ModuleConnectionStatus ModuleStatus { get; }

        event IModuleConnectionStatusChanged ModuleStatusChanged;

        Task Init();
        Task Reconnect();

        Task ProcessCommand(dynamic command);
    }
}
