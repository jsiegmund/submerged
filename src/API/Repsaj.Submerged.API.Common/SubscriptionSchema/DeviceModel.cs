using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public class DeviceModel
    {
        [JsonIgnore]
        private List<ModuleModel> _modules = new List<ModuleModel>();
        [JsonIgnore]
        private List<SensorModel> _sensors = new List<SensorModel>();
        [JsonIgnore]
        private List<RelayModel> _relays = new List<RelayModel>();

        public DevicePropertiesModel DeviceProperties { get; set; }
        public dynamic LastTelemetryData { get; set; }
        public List<ModuleModel> Modules { get { return _modules; } set { _modules = value; } }
        public List<SensorModel> Sensors { get { return _sensors; } set { _sensors = value; } }
        public List<RelayModel> Relays { get { return _relays; } set { _relays = value; } }

        public static DeviceModel BuildDevice(string deviceId, bool isSimulated)
        {
            DeviceModel model = new DeviceModel();

            DevicePropertiesModel properties = new DevicePropertiesModel(deviceId, isSimulated);
            model.DeviceProperties = properties;

            return model;
        }
    }
}
