using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Models
{
    public class GroupedTelemetryModel
    {
        public int Key { get; set; }
        public IEnumerable<SensorTelemetryModel> SensorData { get; set; }
    }
}
