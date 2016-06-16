using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp
{
    public class DeviceTelemetryModel
    {
        public string ObjectType { get { return DeviceMessageObjectTypes.TELEMETRY; } }
        public double? Temperature1 { get; set; }
        public double? Temperature2 { get; set; }
        public double? pH { get; set; }
        public string DeviceId { get; set; }
    }
}
