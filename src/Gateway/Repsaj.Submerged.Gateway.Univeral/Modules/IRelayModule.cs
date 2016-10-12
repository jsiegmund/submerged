using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules
{
    interface IRelayModule
    {
        void SwitchRelay(string port, bool value);
    }
}
