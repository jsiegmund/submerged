using Repsaj.Submerged.GatewayApp.Modules;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Models
{
    public class ModuleDisplayModel : NotificationBase<Module>
    {
        public ModuleDisplayModel(Module module = null) : base(module) { }

        public String Name
        {
            get { return This.Name; }
            set { SetProperty(This.Name, value, () => This.Name = value); }
        }

        public int? DisplayOrder
        {
            get { return This.DisplayOrder; }
            set { SetProperty(This.DisplayOrder, value, () => This.DisplayOrder = value); }
        }

        public string Status
        {
            get { return This.Status; }
            set { SetProperty(This.Status, value, () => This.Status = value); }
        }
    }
}
