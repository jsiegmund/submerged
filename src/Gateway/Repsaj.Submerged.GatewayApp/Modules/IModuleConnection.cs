using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repsaj.Submerged.GatewayApp.Modules.ModuleConnectionBase;

namespace Repsaj.Submerged.GatewayApp.Modules
{
    public delegate void IModuleConnectionStatusChanged(string moduleName, ModuleConnectionStatus oldStatus, ModuleConnectionStatus newStatus);

    public interface IModuleConnection : IDisposable
    {
        string ModuleName { get; }
        ModuleConnectionStatus ModuleStatus { get; }
        string StatusAsText { get; }

        event IModuleConnectionStatusChanged ModuleStatusChanged;

        void Init();
        dynamic RequestArduinoData();
    }
}
