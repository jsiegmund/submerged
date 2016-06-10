using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceTelemetryModel
    {
        public double? Temperature1 { get; set; }
        public double? Temperature2 { get; set; }
        public double? pH { get; set; }
        public bool? LeakDetected { get; set; }
        public string LeakSensors { get; set; }
        public string DeviceId { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
