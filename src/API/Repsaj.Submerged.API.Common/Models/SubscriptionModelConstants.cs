using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Models
{
    public static class ModuleTypes
    {
        public static readonly string SENSORS = "SensorModule";
        public static readonly string CABINET = "CabinetModule";
        public static readonly string FIRMATA = "FirmataModule";
    }

    public static class SensorTypes
    {
        public static readonly string TEMPERATURE = "temperature";
        public static readonly string PH = "pH";
        public static readonly string STOCKFLOAT = "stockfloat";
        public static readonly string MOISTURE = "moisture";
        public static readonly string FLOW = "flow";
    }

}
