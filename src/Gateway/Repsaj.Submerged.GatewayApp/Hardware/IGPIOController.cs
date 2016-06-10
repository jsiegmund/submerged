using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteArduino.Hardware
{
    public interface IGPIOController
    {
        void SetPin(int pinNumber, bool relayState);
    }
}
