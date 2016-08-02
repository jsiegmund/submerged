using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Models;
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

        public override IEnumerable<SensorTelemetryModel> RequestSensorData()
        {
            List<SensorTelemetryModel> sensorData = new List<SensorTelemetryModel>();
            sensorData.Add(new SensorTelemetryModel("leakDetected", false));
            sensorData.Add(new SensorTelemetryModel("leakSensors", ""));
            return sensorData;
        }
    }
}
