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
    }

    public static class SensorTypes
    {
        public static readonly string TEMPERATURE = "temperature";
        public static readonly string PH = "pH";
    }

    public static class CommandNames
    {
        public static readonly string UPDATE_INFO = "UpdateInfo";
        public static readonly string SWITCH_RELAY = "SwitchRelay";
    }
}
