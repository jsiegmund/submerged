using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Arduino
{
    public interface IModuleConnectionFactory
    {
        Task Init();
        ModuleConnection GetModuleConnection(string moduleName);
        ModuleConnection GetModuleConnection(Module module);
    }
}
