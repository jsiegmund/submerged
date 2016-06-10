using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Device
{
    public class DeviceFactory
    {
        public const string OBJECT_TYPE_DEVICE_INFO = "DeviceInfo";

        public const string VERSION_1_0 = "1.0";

        private const int MAX_COMMANDS_SUPPORTED = 6;

        private const bool IS_SIMULATED_DEVICE = false;

        public static dynamic GetDevice(string deviceId, string key)
        {
            dynamic device = DeviceHelper.BuildDeviceStructure(deviceId, true);

            AssignDeviceProperties(deviceId, device);
            device.ObjectType = OBJECT_TYPE_DEVICE_INFO;
            device.Version = VERSION_1_0;
            device.IsSimulatedDevice = IS_SIMULATED_DEVICE;

            //AssignCommands(device);

            return device;
        }

        private static void AssignDeviceProperties(string deviceId, dynamic device)
        {
            dynamic deviceProperties = DeviceHelper.GetDeviceProperties(device);
            deviceProperties.HubEnabledState = true;
            deviceProperties.Manufacturer = "Repsaj Inc.";
            //deviceProperties.ModelNumber = "MD-" + GetIntBasedOnString(deviceId + "ModelNumber", 1000);
            //deviceProperties.SerialNumber = "SER" + GetIntBasedOnString(deviceId + "SerialNumber", 10000);
            //deviceProperties.FirmwareVersion = "1." + GetIntBasedOnString(deviceId + "FirmwareVersion", 100);
            //deviceProperties.Platform = "Plat-" + GetIntBasedOnString(deviceId + "Platform", 100);
            //deviceProperties.Processor = "i3-" + GetIntBasedOnString(deviceId + "Processor", 10000);
            //deviceProperties.InstalledRAM = GetIntBasedOnString(deviceId + "InstalledRAM", 100) + " MB";            
        }

        private static int GetIntBasedOnString(string input, int maxValueExclusive)
        {
            int hash = input.GetHashCode();

            //Keep the result positive
            if (hash < 0)
            {
                hash = -hash;
            }

            return hash % maxValueExclusive;
        }
    }
}