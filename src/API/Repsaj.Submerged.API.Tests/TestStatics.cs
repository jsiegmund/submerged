using Repsaj.Submerged.API.Tests.Helpers;
using Repsaj.Submerged.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests
{
    public class TestStatics
    {
        public static readonly string subscription_name = TestConfigHelper.SubscriptionName;
        public static readonly string subscription_user = TestConfigHelper.SubscriptionUser;
        public static readonly string subscription_description = "[TEST] Subscription description";

        public static readonly string device_id = TestConfigHelper.DeviceId;
        public static readonly bool device_isSimulated = true;
        public static readonly string device_name = "[TEST] Device Name";
        public static readonly string device_description = "[TEST] Device Description";

        public static readonly string module_name = "[TEST] Module Name";
        public static readonly string module_description = "[TEST] Module Description";
        public static readonly string module_connectionString = "[TEST] Module connection string";
        public static readonly string module_moduleType = ModuleTypes.SENSORS;

        public static readonly string sensor_name = "[TEST] Sensor Name";
        public static readonly string sensor_displayName = "[TEST] Sensor Display Name";
        public static readonly string sensor_description = "[TEST] Sensor Description";
        public static readonly string sensor_type = SensorTypes.TEMPERATURE;
        public static readonly string[] sensor_pinConfig = new string[] { "D0" };

        public static readonly int relay_number = 0;
        public static readonly string relay_name = "[TEST] Relay Name";
        public static readonly string[] relay_pinConfig = new string[] { "D0" };

        public static readonly string tankLog_title = "[TEST] Test log line";
        public static readonly string tankLog_description = "[TEST] Test performing maintenance on the tank";
        public static readonly string tankLog_logType = "Maintenance";
        public static readonly Guid tankLog_id = new Guid("{7D986651-7CB7-494E-84E5-E75350F583FE}");
        public static readonly Guid tankLog_tankId = new Guid("{9AB89AAC-715A-4B5C-A331-6B358744B7A0}");
    }
}
