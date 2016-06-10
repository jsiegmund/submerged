using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp
{

    public class ArduinoException : Exception
    {
        public  ArduinoException() { }
        public  ArduinoException(string message) : base(message) { }
        public  ArduinoException(string message, Exception inner) : base(message, inner) { }       
    }
}
