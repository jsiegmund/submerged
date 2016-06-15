using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Models
{
    public class DeviceModel
    {
        [JsonIgnore]
        private List<Module> _modules = new List<Module>();
        [JsonIgnore]
        private List<Sensor> _sensors = new List<Sensor>();
        [JsonIgnore]
        private List<Relay> _relays = new List<Relay>();

        public DevicePropertiesModel DeviceProperties { get; set; }
        public dynamic LastTelemetryData { get; set; }
        public List<Module> Modules { get { return _modules; } set { _modules = value; } }
        public List<Sensor> Sensors { get { return _sensors; } set { _sensors = value; } }
        public List<Relay> Relays { get { return _relays; } set { _relays = value; } }

        public static DeviceModel BuildDevice(string deviceId, bool isSimulated)
        {
            DeviceModel model = new DeviceModel();

            DevicePropertiesModel properties = new DevicePropertiesModel(deviceId, isSimulated);
            model.DeviceProperties = properties;

            return model;
        }
    }
}
