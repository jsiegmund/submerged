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
        public DateTime? OutTime { get; set; }
        public DateTime? Timestamp { get; set; }
        public double? AverageTemp1 { get; set; }
        public double? MinimumTemp1 { get; set; }
        public double? MaximumTemp1 { get; set; }
        public double? AverageTemp2 { get; set; }
        public double? MinimumTemp2 { get; set; }
        public double? MaximumTemp2 { get; set; }
        public double? AveragePH { get; set; }
        public double? MinimumPH { get; set; }
        public double? MaximumPH { get; set; }
    }
}
