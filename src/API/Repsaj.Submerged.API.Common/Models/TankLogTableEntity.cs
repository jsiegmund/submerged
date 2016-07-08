using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Models
{
    public class TankLogTableEntity : TableEntity
    {
        public TankLogTableEntity(Guid tankId, string logId)
        {
            this.PartitionKey = tankId.ToString();
            this.RowKey = logId;
        }

        public TankLogTableEntity() { }

        [IgnoreProperty]
        public string LogId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        [IgnoreProperty]
        public Guid TankId
        {
            get { return new Guid(PartitionKey); }
            set { PartitionKey = value.ToString(); }
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime TimeCreated { get; set; }
        public string LogType { get; set; }
    }
}
