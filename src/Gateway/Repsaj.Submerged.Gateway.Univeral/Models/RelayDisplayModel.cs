using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class RelayDisplayModel : NotificationBase
    {
        public RelayDisplayModel(Relay relay) : base()
        {
            this._name = relay.Name;
            this._state = relay.State;
            this._orderNumber = relay.OrderNumber;
        }

        string _name;
        public String Name
        {
            get { return this._name; }
            set { SetProperty(this._name, value, () => this._name = value); }
        }

        bool? _state;
        public bool? State
        {
            get { return this._state; }
            set { SetProperty(this._state, value, () => this._state = value); this.RaisePropertyChanged(nameof(RelayStateAsText)); }
        }

        int? _orderNumber;
        public int? OrderNumber
        {
            get { return this._orderNumber; }
            set { SetProperty(this._orderNumber, value, () => this._orderNumber = value); }
        }

        public string RelayStateAsText
        {
            get
            {
                if (this.State.HasValue)
                    return this.State.Value ? "ON" : "OFF";
                else
                    return String.Empty;
            }
        }
    }
}
