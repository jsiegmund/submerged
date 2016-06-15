using Microsoft.Maker.Serial;
using Repsaj.Submerged.GatewayApp.Universal.Exceptions;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;

namespace Repsaj.Submerged.GatewayApp.Arduino
{
    public class ModuleConnectionFactory : IModuleConnectionFactory
    {
        DeviceInformationCollection _devices;
        Dictionary<string, ModuleConnection> _connections = new Dictionary<string, ModuleConnection>();

        public ModuleConnectionFactory()
        {
        }

        public async Task Init()
        {
            try
            {
                _devices = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort)).AsTask();
                int count = _devices.Count;
            }
            catch (Exception ex)
            {
                string msg = $"Failure initializing device registry. Restart the application and try again";
                //MinimalEventSource.Log.LogError(msg);
                throw;
            }
        }

        public ModuleConnection GetModuleConnection(string moduleName)
        {
            return _connections.Values.SingleOrDefault(m => m.ModuleName == moduleName);
        }

        public ModuleConnection GetModuleConnection(Module module)
        {
            ModuleConnection connection = null;

            if (_connections.ContainsKey(module.ConnectionString))
                return _connections[module.ConnectionString];

            try
            {
                var device = _devices.SingleOrDefault(d => d.Id == module.ConnectionString);

                if (device == null)
                {
                    string msg = $"Could not find module {module.Name} in the device registry. Check the connectionstring or repair the device";
                    //MinimalEventSource.Log.LogWarning(msg);
                    return null;
                }

                if (module.ModuleType == ModuleTypes.CABINET)
                    connection = new CabinetModuleConnection(device, module.Name);
                else if (module.ModuleType == ModuleTypes.SENSORS)
                    connection = new SensorModuleConnection(device, module.Name);
                else
                    throw new ArgumentException($"Module type {module.ModuleType} is not supported by this device.");

                connection.Init();

                // add to the list of managed connections
                _connections.Add(module.ConnectionString, connection);
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogWarning($"Failure initializing connection to module {moduleConfig.Name}: {ex}");
            }

            return connection;
        }
    }
}
