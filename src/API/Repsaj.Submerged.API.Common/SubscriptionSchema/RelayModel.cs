using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public class RelayModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Boolean State { get; set; }
        public int? OrderNumber { get; set; }
        public string Module { get; set; }
        public bool ToggleForMaintenance { get; set; }
        public string[] PinConfig { get; set; } 

        public static RelayModel BuildModel(string name, string displayName, string moduleName, string[] pinConfig)
        {
            RelayModel relay = new RelayModel();
            relay.Name = name;
            relay.DisplayName = displayName;
            relay.State = true;
            relay.Module = moduleName;
            relay.PinConfig = pinConfig;            
            return relay;
        }
    }
}
