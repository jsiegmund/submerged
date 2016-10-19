using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Modules.Simulated;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.LED
{
    class SimulatedLedenetModuleConnection : SimulatedModuleConnectionBase
    {
        public override string ModuleType
        {
            get
            {
                return ModuleTypes.LEDENET;
            }
        }

        public SimulatedLedenetModuleConnection(string name): base(name)
        {
            
        }
        
    }
}
