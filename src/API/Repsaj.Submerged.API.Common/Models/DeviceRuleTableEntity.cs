using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Models
{
    public class DeviceRuleTableEntity : TableEntity
    {
        public DeviceRuleTableEntity(string deviceId, string ruleId)
        {
            this.PartitionKey = deviceId;
            this.RowKey = ruleId;
        }

        public DeviceRuleTableEntity() { }

        [IgnoreProperty]
        public string DeviceId
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        [IgnoreProperty]
        public string RuleId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        public string DataField { get; set; }

        public string DataType { get; set; }

        public double Threshold { get; set; }

        public string Operator { get; set; }

        public string RuleOutput { get; set; }

        public bool Enabled { get; set; }

        public string RuleName { get; set; }
    }
}
