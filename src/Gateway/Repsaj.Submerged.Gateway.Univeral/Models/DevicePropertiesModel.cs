using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class DevicePropertiesModel
    {
        public string DeviceID { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public bool IsSimulatedDevice { get; set; }
        public int? DisplayOrder { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }


        public DevicePropertiesModel(string deviceId, bool isSimulated)
        {
            this.DeviceID = deviceId;
            this.IsSimulatedDevice = isSimulated;
            this.CreatedTime = DateTime.UtcNow;
            this.UpdatedTime = DateTime.UtcNow;
        }
    }
}
