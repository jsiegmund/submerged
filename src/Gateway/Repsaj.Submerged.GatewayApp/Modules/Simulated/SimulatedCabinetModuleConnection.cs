using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Modules.Simulated
{
    class SimulatedCabinetModuleConnection : SimulatedModuleConnectionBase
    {
        public SimulatedCabinetModuleConnection(string moduleName) : base(moduleName)
        {

        }

        public override string ModuleType
        {
            get
            {
                return ModuleTypeDisplayNames.CABINET;
            }
        }

        public override dynamic RequestArduinoData()
        {
            dynamic data = new System.Dynamic.ExpandoObject();
            data.leakDetected = false;
            data.leakSensors = "";
            return data;
        }
    }
}
