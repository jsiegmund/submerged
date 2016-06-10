using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Models
{
    public class DeviceCommandModel
    {
        public string Name { get; set; }
        public string MessageId{ get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public dynamic Parameters { get; set; }

        public static DeviceCommandModel BuildDeviceCommand(string name)
        {
            DeviceCommandModel model = new DeviceCommandModel();
            model.CreatedTime = DateTime.Now;
            model.Name = name;
            model.MessageId = Guid.NewGuid().ToString();
            return model;
        }
    }
}
    