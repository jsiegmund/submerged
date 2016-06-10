using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.SimulatedDevice
{
    public class DeviceTelemetryModel
    {
        public double? temperature1 { get; set; }
        public double? temperature2 { get; set; }
        public double? pH { get; set; }
        public bool leakDetected { get; set; }
        public string leakSensors { get; set; }
    }
}
