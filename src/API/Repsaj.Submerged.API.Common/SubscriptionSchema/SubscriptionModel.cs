using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public class SubscriptionModel
    {
        [JsonIgnore]
        private List<TankModel> _tanks = new List<TankModel>();
        [JsonIgnore]
        private List<DeviceModel> _devices = new List<DeviceModel>();

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public SubscriptionPropertiesModel SubscriptionProperties;
        public List<TankModel> Tanks { get { return _tanks; } set { _tanks = value; } }
        public List<DeviceModel> Devices { get { return _devices; } set { _devices = value; } }

        public SubscriptionModel() { }

        public static SubscriptionModel BuildSubscription(Guid id, string name, string description, string user)
        {
            SubscriptionModel model = new SubscriptionModel();
            model.SubscriptionProperties = new SubscriptionPropertiesModel()
            {
                SubscriptionID = id,
                Name = name,
                Description = description,
                User = user,
                CreatedTime = DateTime.Now
            };

            model.Tanks = new List<TankModel>();
            model.Devices = new List<DeviceModel>();

            return model;
        }
    }
}
