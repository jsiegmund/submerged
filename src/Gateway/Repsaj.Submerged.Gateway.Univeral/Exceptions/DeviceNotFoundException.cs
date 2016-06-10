using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Exceptions
{
    public class DeviceNotFoundException : Exception
    {
        public DeviceNotFoundException() { }
        public DeviceNotFoundException(string message) : base(message) { }
        public DeviceNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}