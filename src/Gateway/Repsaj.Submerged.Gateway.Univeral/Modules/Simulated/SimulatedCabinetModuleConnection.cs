using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.Simulated
{
    class SimulatedCabinetModuleConnection : SimulatedModuleConnectionBase, ISensorModule, IRelayModule
    {
        private Sensor[] _sensors;
        private Relay[] _relays;

        public IEnumerable<Sensor> Sensors
        {
            get { return _sensors; }
        }

        public SimulatedCabinetModuleConnection(string moduleName, Sensor[] sensors, Relay[] relays) : base(moduleName)
        {
            this._sensors = sensors;
            this._relays = relays;
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

        public void SwitchRelay(String port, Boolean value)
        {
            throw new NotImplementedException();
        }
    }
}
