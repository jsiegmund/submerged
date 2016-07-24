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
        public string[] PinConfig { get; set; }

        public static SensorModel BuildSensor(string name, string displayName, string sensorType, string moduleName, string[] pinConfig)
        {
            SensorModel sensor = new SensorModel()
            {
                Name = name,
                DisplayName = displayName,
                Module = moduleName,
                SensorType = sensorType,
                PinConfig = pinConfig
            };

            return sensor;
        }

        public static object BuildSensor(string sensor_name, string sensor_description, string pH, string module_name, object sensor_pinConfig)
        {
            throw new NotImplementedException();
        }
    }
}
