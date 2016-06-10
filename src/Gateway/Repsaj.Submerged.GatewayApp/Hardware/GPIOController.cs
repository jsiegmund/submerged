using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RemoteArduino.Hardware
{
    public class GPIOController : IGPIOController
    {
        GpioController _controller;

        public GPIOController()
        {
            InitGPIO();
        }

        private void InitGPIO()
        {
            _controller = GpioController.GetDefault();

            if (_controller == null)
            {
                throw new HardwareException("Could not find the GPIO controller.");
            }
        }

        private GpioPin GetPin(int pinNumber)
        {
            return _controller.OpenPin(pinNumber);
        }

        public void SetPin(int pinNumber, bool high)
        {
            try
            {
                using (GpioPin pin = GetPin(pinNumber))
                {
                    pin.Write(high ? GpioPinValue.High : GpioPinValue.Low);
                    pin.SetDriveMode(GpioPinDriveMode.Output);
                }
            }
            catch (Exception ex)
            {
                throw new HardwareException(String.Format("Could not switch pin {0}: {1}", pinNumber, ex.Message), ex);
            }
        }
    }
}
