using Repsaj.Submerged.GatewayApp.Universal.Modules;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class ModuleDisplayModel : NotificationBase
    {
        public ModuleDisplayModel(Module module) : base()
        {
            _name = module.Name;
            _displayOrder = module.DisplayOrder;
            _status = module.Status;
        }

        string _name;
        public String Name
        {
            get { return this._name; }
            set { SetProperty(this._name, value, () => this._name = value); }
        }

        int? _displayOrder;
        public int? DisplayOrder
        {
            get { return this._displayOrder; }
            set { SetProperty(_displayOrder, value, () => _displayOrder = value); }
        }

        string _status;
        public string Status
        {
            get { return this._status; }
            set { SetProperty(this._status, value, () => this._status = value); }
        }
    }
}
