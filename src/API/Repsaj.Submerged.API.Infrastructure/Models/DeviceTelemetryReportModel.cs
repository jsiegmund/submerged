using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceTelemetryReportModel
    {
        public IEnumerable<string> DataLabels { get; set; }
        public IEnumerable<string> SerieLabels { get; set; }
        public IEnumerable<IEnumerable<double?>> DataSeries { get; set; }
    }
}
