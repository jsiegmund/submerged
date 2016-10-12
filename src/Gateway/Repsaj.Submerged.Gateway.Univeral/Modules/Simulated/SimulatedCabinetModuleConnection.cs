using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.Simulated
{
    class SimulatedCabinetModuleConnection : SimulatedModuleConnectionBase, ISensorModule
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

        public Task<IEnumerable<SensorTelemetryModel>> RequestSensorData()
        {
            List<SensorTelemetryModel> sensorData = new List<SensorTelemetryModel>();
            sensorData.Add(new SensorTelemetryModel("leakDetected", false));
            sensorData.Add(new SensorTelemetryModel("leakSensors", ""));

            IEnumerable<SensorTelemetryModel> result = sensorData;
            return Task.FromResult(result);
        }

        public override void SwitchRelay(string name, bool high)
        {
            
        }
    }
}
