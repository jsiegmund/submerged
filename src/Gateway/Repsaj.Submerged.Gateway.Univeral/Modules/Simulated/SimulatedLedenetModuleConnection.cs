using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Models;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.Simulated
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

        public override void SwitchRelay(string name, bool high)
        {
            throw new NotImplementedException();
        }

        
    }
}
