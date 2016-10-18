using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Commands;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
{
    interface IRelayModule: IModuleConnection
    {
        void SwitchRelay(string port, bool value);
    }
}
