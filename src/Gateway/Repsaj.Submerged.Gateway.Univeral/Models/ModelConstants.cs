using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public static class ModuleTypes
    {
        public static readonly string SENSORS = "SensorModule";
        public static readonly string CABINET = "CabinetModule";
        public static readonly string FIRMATA = "FirmataModule";
        public static readonly string LEDENET = "LedenetModule";
    }

    public static class ModuleTypeDisplayNames
    {
        public static readonly string SENSORS = "Sensor Module";
        public static readonly string CABINET = "Cabinet Module";
    }

    public static class SensorTypes
    {
        public static readonly string TEMPERATURE = "temperature";
        public static readonly string PH = "pH";
        public static readonly string STOCKFLOAT = "stockfloat";
        public static readonly string MOISTURE = "moisture";
        public static readonly string FLOW = "flow";
    }

    public static class CommandNames
    {
        public static readonly string UPDATE_INFO = "UpdateInfo";
        public static readonly string SWITCH_RELAY = "SwitchRelay";
        public static readonly string MODULE_COMMAND = "ModuleCommand";
    }
}
