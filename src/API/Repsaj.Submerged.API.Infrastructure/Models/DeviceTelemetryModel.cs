using Repsaj.Submerged.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceTelemetryModel
    {
        public string DeviceId { get; set; }
        public DateTimeOffset? EventEnqueuedUTCTime { get; set; }
        public SensorTelemetryModel[] SensorData { get; set; }
    }
}
