using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public class TankLog
    {
        public TankLog()
        {
            TimeCreated = DateTime.UtcNow;
        }

        public TankLog(Guid tankId)
        {
            TankId = tankId;
            LogId = Guid.NewGuid();
            TimeCreated = DateTime.UtcNow;
        }

        public Guid LogId { get; set; }
        public Guid TankId { get; set; }
        public string ETag { get; set; }
        public DateTime TimeCreated { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string LogType { get; set; }

        public static TankLog BuildLog(Guid tankId, string title, string type, string description)
        {
            TankLog newLog = new TankLog(tankId)
            {
                Title = title,
                LogType = type,
                Description = description
            };

            return newLog;
        }
    }
}
