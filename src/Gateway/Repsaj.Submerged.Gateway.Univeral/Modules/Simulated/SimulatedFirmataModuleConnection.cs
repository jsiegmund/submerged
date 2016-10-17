using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Helpers;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.Simulated
{
    class SimulatedFirmataModuleConnection : SimulatedModuleConnectionBase, ISensorModule
    {
        Sensor[] _sensors;
        Relay[] _relays;

        double basePh = 7.0;
        double rangePh = 1.2;
        double baseTemp = 24;
        double rangeTemp = 5;

        public IEnumerable<Sensor> Sensors
        {
            get { return _sensors; }
        }

        public SimulatedFirmataModuleConnection(string moduleName, Sensor[] sensors, Relay[] relays) : base(moduleName)
        {
            _sensors = sensors;
            _relays = relays;
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

            foreach (Sensor sensor in _sensors)
            {
                object value = null;

                if (sensor.SensorType == SensorTypes.TEMPERATURE)
                    value = Randomizer.GetRandomDouble(baseTemp, rangeTemp);
                else if (sensor.SensorType == SensorTypes.PH)
                    value = Randomizer.GetRandomDouble(basePh, rangePh);
                else if (sensor.SensorType == SensorTypes.STOCKFLOAT)
                    value = Randomizer.GetRandomBool();
                else if (sensor.SensorType == SensorTypes.FLOW)
                    value = Randomizer.GetRandomDouble(1200, 200);
                else if (sensor.SensorType == SensorTypes.MOISTURE)
                    value = Randomizer.GetRandomBool();

                sensorData.Add(new SensorTelemetryModel(sensor.Name, value));

            }

            IEnumerable<SensorTelemetryModel> result = sensorData;
            return Task.FromResult(result);
        }

        public override void SwitchRelay(string name, bool high)
        {
            
        }
    }
}
