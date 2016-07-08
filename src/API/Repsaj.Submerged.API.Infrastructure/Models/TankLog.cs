using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class TankLog
    {
        public TankLog()
        {
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
    }
}
