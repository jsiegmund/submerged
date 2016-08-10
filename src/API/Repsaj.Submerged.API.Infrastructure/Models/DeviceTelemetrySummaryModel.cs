using Repsaj.Submerged.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceTelemetrySummaryModel
    {
        public string DeviceId { get; set; }
        public DateTimeOffset? OutTime { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public string SensorName { get; set; }
        public double AvgSensorValue { get; set; }
    }
}
