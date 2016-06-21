using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Modules.Simulated
{
    public class SimulatedSensorModuleConnection : SimulatedModuleConnectionBase
    {

        double basePh = 7.0;
        double baseTemp1 = 24;
        double baseTemp2 = 20;

        public SimulatedSensorModuleConnection(string moduleName) : base(moduleName)
        {

        }

        public override string ModuleType
        {
            get
            {
                return ModuleTypeDisplayNames.SENSORS;
            }
        }

        public override dynamic RequestArduinoData()
        {
            Random rand = new Random();

            double currentTemp1 = baseTemp1 + rand.NextDouble() * 5 - 2.5;
            double currentTemp2 = baseTemp2 + rand.NextDouble() * 5 - 2.5;
            double currentPH = basePh + rand.NextDouble() * 1.2 - 0.6;

            dynamic data = new ExpandoObject();
            data.temperature1 = currentTemp1;
            data.temperature2 = currentTemp2;
            data.pH = currentPH;
            return data;

        }
    }
}
