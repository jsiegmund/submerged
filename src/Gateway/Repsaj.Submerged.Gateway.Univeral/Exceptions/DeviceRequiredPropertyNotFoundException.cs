using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Exceptions
{
    public class DeviceRequiredPropertyNotFoundException : Exception
    {
        public DeviceRequiredPropertyNotFoundException() { }
        public DeviceRequiredPropertyNotFoundException(string message) : base(message) { }
        public DeviceRequiredPropertyNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
