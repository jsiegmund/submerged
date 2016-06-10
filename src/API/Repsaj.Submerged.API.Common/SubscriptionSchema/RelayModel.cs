﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public class RelayModel
    {
        public int RelayNumber { get; set; }
        public string Name { get; set; }
        public Boolean State { get; set; }
        public int? OrderNumber { get; set; }

        public static RelayModel BuildModel(int relayNumber, string name)
        {
            RelayModel relay = new RelayModel();
            relay.RelayNumber = relayNumber;
            relay.Name = name;
            relay.State = true;
            return relay;
        }
    }
}
