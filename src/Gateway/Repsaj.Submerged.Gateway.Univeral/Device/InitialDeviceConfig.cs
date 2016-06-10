using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Device
{
    /// <summary>
    /// Device config that is read from a repository to init a set of devices
    /// in a single simulator for testing.
    /// </summary>
    public class InitialDeviceConfig
    {
        /// <summary>
        /// IoT Hub HostName
        /// </summary>
        public string HostName { get; set; }
        public string DeviceId { get; set; }
        public string Key { get; set; }
    }
}
