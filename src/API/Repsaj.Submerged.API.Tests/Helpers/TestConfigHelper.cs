using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests.Helpers
{
    public static class TestConfigHelper
    {
        public static string DeviceId
        {
            get { return ConfigurationManager.AppSettings["deviceId"]; }
        }

        public static string SubscriptionUser
        {
            get { return ConfigurationManager.AppSettings["subscriptionUser"]; }
        }

        public static string SubscriptionName
        {
            get { return ConfigurationManager.AppSettings["subscriptionName"]; }
        }

        public static string TankName
        {
            get { return ConfigurationManager.AppSettings["tankName"]; }
        }


    }
}
