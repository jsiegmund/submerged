using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public class SubscriptionPropertiesModel
    {
        public Guid SubscriptionID { get; set; }
        public string Name { get; set; }
        public string User { get; set; }
        public string Description { get; internal set; }
        public bool SubscriptionEnabledState { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
