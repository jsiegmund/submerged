using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Modules.Simulated
{
    class SimulatedFirmataModuleConnection : SimulatedModuleConnectionBase
    {
        Sensor[] _sensors;
        Relay[] _relays;

        Random _rand = new Random();

        double basePh = 7.0;
        double rangePh = 1.2;
        double baseTemp = 24;
        double rangeTemp = 5;

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

        public override JObject RequestArduinoData()
        {
            JObject data = new JObject();

            foreach (Sensor sensor in _sensors)
            {
                object value = null;

                if (sensor.SensorType == SensorTypes.TEMPERATURE)
                    value = GetRandomDouble(baseTemp, rangeTemp);
                else if (sensor.SensorType == SensorTypes.PH)
                    value = GetRandomDouble(basePh, rangePh);
                else if (sensor.SensorType == SensorTypes.STOCKFLOAT)
                    value = GetRandomBool();

                data.Add(new JProperty(sensor.Name, value));

            }

            return data;
        }

        private double GetRandomDouble(double baseValue, double rangeValue)
        {
            return baseValue + _rand.NextDouble() * rangeValue - (rangeValue / 2);
        }

        private bool GetRandomBool()
        {
            return _rand.NextDouble() > 0.5;
        }
    }
}
