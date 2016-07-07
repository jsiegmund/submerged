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
        public static readonly string module_connectionString = "[TEST] Module connection string";
        public static readonly string module_moduleType = ModuleTypes.SENSORS;

        public static readonly string sensor_name = "[TEST] Sensor Name";
        public static readonly string sensor_displayName = "[TEST] Sensor Display Name";
        public static readonly string sensor_type = SensorTypes.TEMPERATURE;

        public static readonly int relay_number = 0;
        public static readonly string relay_name = "[TEST] Relay Name";

    }
}
