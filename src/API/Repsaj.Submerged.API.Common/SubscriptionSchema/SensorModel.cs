using Repsaj.Submerged.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public class SensorModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int? OrderNumber { get; set; }
        public double? MinThreshold { get; set; }
        public double? MaxThreshold { get; set; }
        public bool MinThresholdEnabled { get; set; }
        public bool MaxThresholdEnabled { get; set; }
        public string SensorType { get; set; }
        public string Module { get; set; }

        public static SensorModel BuildSensor(string name, string displayName, string sensorType, string moduleName)
        {
            SensorModel sensor = new SensorModel()
            {
                Name = name,
                DisplayName = displayName,
                Module = moduleName
            };

            return sensor;
        }
    }
}
