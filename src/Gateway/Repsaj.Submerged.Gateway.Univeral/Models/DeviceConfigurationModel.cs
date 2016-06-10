using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class DeviceConfigurationModel
    {
        public string IoTHubHostname { get; set; }
        public string DeviceId { get; set; }
        public string DeviceKey { get; set; }
    }
}
