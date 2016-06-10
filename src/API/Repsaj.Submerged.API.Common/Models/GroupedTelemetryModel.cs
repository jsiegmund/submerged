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
        public double? Temperature1 { get; set; }
        public double? Temperature2 { get; set; }
        public double? pH { get; set; }
    }
}
