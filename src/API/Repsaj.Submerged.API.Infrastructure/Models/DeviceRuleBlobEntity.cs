using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceRuleBlobEntity
    {
        public DeviceRuleBlobEntity(string deviceId)
        {
            DeviceId = deviceId;
            this.SensorRules = new List<SensorRuleEntity>();
        }

        public string DeviceId { get; private set; }
        public List<SensorRuleEntity> SensorRules { get; set; }
    }

    public class SensorRuleEntity
    {
        public SensorRuleEntity() { }
        public SensorRuleEntity(string sensorName, object threshold, string @operator)
        {
            this.SensorName = sensorName;
            this.Threshold = threshold;
            this.Operator = @operator;
        }

        public string SensorName { get; set; }
        public object Threshold { get; set; }
        public string Operator { get; set; }
    }
}
