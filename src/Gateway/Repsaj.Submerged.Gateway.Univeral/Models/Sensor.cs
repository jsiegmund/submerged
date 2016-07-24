using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class Sensor
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
        public object Reading { get; set; }
        public string Module { get; set; }
        public string[] PinConfig { get; set; }

        public static Sensor BuildSensor(string name, string displayName, string sensorType)
        {
            // maybe switch this for an enum, at least solve it differently
            if (sensorType != SensorTypes.TEMPERATURE && sensorType != SensorTypes.PH)
                throw new ArgumentException($"Sensor type {sensorType} is not a valid type", "sensorType");

            Sensor sensor = new Sensor()
            {
                Name = name,
                DisplayName = displayName
            };

            return sensor;
        }
    }
}
