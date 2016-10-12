using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
{
    public interface IModuleConnectionFactory
    {
        Task Init();
        IModuleConnection GetModuleConnection(string moduleName);
        IModuleConnection GetModuleConnection(Module module, Sensor[] sensors, Relay[] relays);
    }
}
