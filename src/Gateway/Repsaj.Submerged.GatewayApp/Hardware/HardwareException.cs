using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteArduino.Hardware
{
    public class HardwareException : Exception
    {
        public HardwareException() { }
        public HardwareException(string message) : base(message) { }
        public HardwareException(string message, Exception inner) : base(message, inner) { }
    }
}