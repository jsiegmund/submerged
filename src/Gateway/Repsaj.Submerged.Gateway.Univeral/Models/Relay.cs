using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class Relay
    {
        public int RelayNumber { get; set; }
        public string Name { get; set; }
        public Boolean State { get; set; }
        public int? OrderNumber { get; set; }

        public static Relay BuildModel(int relayNumber, string name)
        {
            Relay relay = new Relay();
            relay.RelayNumber = relayNumber;
            relay.Name = name;
            relay.State = true;
            return relay;
        }
    }
}
