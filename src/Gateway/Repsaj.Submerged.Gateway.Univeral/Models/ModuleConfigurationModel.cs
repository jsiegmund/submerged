using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class ModuleConfigurationModel
    {
        public string Name { get; set; }
        public string ModuleType { get; set; }
        public string ConnectionString { get; set; }
    }

    public static class ModuleTypes
    {
        public static readonly string CABINET = "CabinetModule";
        public static readonly string SENSORS = "SensorModule";
    }
}
