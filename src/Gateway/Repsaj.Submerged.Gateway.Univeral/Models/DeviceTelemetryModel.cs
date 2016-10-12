using Newtonsoft.Json;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class DeviceTelemetryModel
    {
        public string ObjectType { get { return DeviceMessageObjectTypes.TELEMETRY; } }
        public string DeviceId { get; set; }
        public IEnumerable<SensorTelemetryModel> SensorData { get; set; }
    }

    public class SensorTelemetryModel
    {
        public SensorTelemetryModel(string sensorName, object value)
        {
            this.SensorName = sensorName;
            this.Value = value;
        }

        public string SensorName { get; set; }
        public object Value { get; set; }
    }
}
