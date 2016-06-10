using Repsaj.Submerged.GatewayApp.Universal.Exceptions;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Device
{
    public static class DeviceHelper
    {
        /// <summary>
        /// Gets a DeviceProperties instance from a Device.
        /// </summary>
        /// <param name="device">
        /// The Device from which to extract a DeviceProperties instance.
        /// </param>
        /// <returns>
        /// A DeviceProperties instance, extracted from <paramref name="device"/>.
        /// </returns>
        public static dynamic GetDeviceProperties(dynamic device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = device.DeviceProperties;

            if (props == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }

            return props;
        }

        /// <summary>
        /// Build a valid device representation in the dynamic format used throughout the app.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="isSimulated"></param>
        /// <returns></returns>
        public static dynamic BuildDeviceStructure(string deviceId, bool isSimulated)
        {
            JObject device = new JObject();

            JObject deviceProps = new JObject();
            deviceProps.Add(DevicePropertiesConstants.DEVICE_ID, deviceId);
            deviceProps.Add(DevicePropertiesConstants.HUB_ENABLED_STATE, null);
            deviceProps.Add(DevicePropertiesConstants.CREATED_TIME, DateTime.UtcNow);
            deviceProps.Add(DevicePropertiesConstants.DEVICE_STATE, "normal");
            deviceProps.Add(DevicePropertiesConstants.UPDATED_TIME, null);

            device.Add(DeviceModelConstants.DEVICE_PROPERTIES, deviceProps);
            device.Add(DeviceModelConstants.COMMANDS, new JArray());
            device.Add(DeviceModelConstants.COMMAND_HISTORY, new JArray());
            device.Add(DeviceModelConstants.MODULE_STATUS, new JArray());
            device.Add(DeviceModelConstants.IS_SIMULATED_DEVICE, isSimulated);

            return device;
        }     
    }
}
