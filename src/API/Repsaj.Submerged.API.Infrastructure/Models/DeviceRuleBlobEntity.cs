using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceRuleBlobEntity
    {
        public DeviceRuleBlobEntity(string deviceId)
        {
            DeviceId = deviceId;
        }

        public string DeviceId { get; private set; }
        public double? Temperature1Min { get; set; }
        public double? Temperature1Max { get; set; }
        public double? Temperature2Min { get; set; }
        public double? Temperature2Max { get; set; }
        public double? pHMin { get; set; }
        public double? pHMax { get; set; }
    }
}
