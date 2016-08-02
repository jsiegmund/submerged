using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Modules
{
    public delegate void IModuleConnectionStatusChanged(string moduleName, ModuleConnectionStatus oldStatus, ModuleConnectionStatus newStatus);

    public interface IModuleConnection : IDisposable
    {
        string ModuleName { get; }
        ModuleConnectionStatus ModuleStatus { get; }

        event IModuleConnectionStatusChanged ModuleStatusChanged;

        void Init();
        IEnumerable<SensorTelemetryModel> RequestSensorData();        
    }
}
