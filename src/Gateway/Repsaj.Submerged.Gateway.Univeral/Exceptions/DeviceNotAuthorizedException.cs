using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Exceptions
{

    public class DeviceNotAuthorizedException : Exception
    {
        public DeviceNotAuthorizedException() { }
        public DeviceNotAuthorizedException(string message) : base(message) { }
        public DeviceNotAuthorizedException(string message, Exception inner) : base(message, inner) { }
    }
}
