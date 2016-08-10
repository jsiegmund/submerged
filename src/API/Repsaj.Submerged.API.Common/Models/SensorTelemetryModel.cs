using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Models
{
    public class SensorTelemetryModel
    {
        public SensorTelemetryModel() { }
        public SensorTelemetryModel(string name, object value)
        {
            this.SensorName = name;
            this.Value = value;
        }

        public string SensorName { get; set; }
        public object Value { get; set; }
    }
}
