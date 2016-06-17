using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Models
{
    public class ModuleModel : NotificationBase<Module>
    {
        public ModuleModel(Module module = null) : base(module) { }

        public String Name
        {
            get { return This.Name; }
            set { SetProperty(This.Name, value, () => This.Name = value); }
        }
        public string Status
        {
            get { return This.Status; }
            set { SetProperty(This.Status, value, () => This.Status = value); }
        }

        public int? DisplayOrder
        {
            get { return This.DisplayOrder; }
            set { SetProperty(This.DisplayOrder, value, () => This.DisplayOrder = value); }
        }
    }
}
