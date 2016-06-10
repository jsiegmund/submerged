using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;

namespace Repsaj.Submerged.GatewayApp.Arduino
{
    public class BluetoothAdapter
    {
        public BluetoothAdapter()
        {

        }

        public DeviceInformation DeviceInformation { get; set; }
        public RfcommDeviceService RfCommDeviceService { get; set; }
        public StreamSocket StreamSocket { get; set; }
    }
}
