using Repsaj.Submerged.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceWithKeys
    {
        public dynamic Device { get; set; }
        public SecurityKeys SecurityKeys { get; set; }

        public DeviceWithKeys(dynamic device, SecurityKeys securityKeys)
        {
            Device = device;
            SecurityKeys = securityKeys;
        }
    }
}
