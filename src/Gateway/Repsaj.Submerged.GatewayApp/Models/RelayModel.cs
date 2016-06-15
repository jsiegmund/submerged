using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Models
{
    public class RelayModel : NotificationBase<Relay>
    {
        public RelayModel(Relay relay = null) : base(relay) { }

        public String Name
        {
            get { return This.Name; }
            set { SetProperty(This.Name, value, () => This.Name = value); }
        }
        public bool State
        {
            get { return This.State; }
            set { SetProperty(This.State, value, () => This.State = value); }
        }
    }
}
