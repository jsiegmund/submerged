using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Device
{
    public class TelemetryHelper
    {
        public static JObject MakeTelemetryObject(JObject data, string deviceId)
        {
            data.Add(DeviceModelConstants.OBJECT_TYPE, DeviceMessageObjectTypes.TELEMETRY);
            data.Add(DevicePropertiesConstants.DEVICE_ID, deviceId);

            return data;
        }

        public static JObject Merge(JObject data, dynamic mergee)
        {
            JObject toMerge = JObject.FromObject(mergee);
            data.Merge(toMerge);
            return data;
        }
    }
}
