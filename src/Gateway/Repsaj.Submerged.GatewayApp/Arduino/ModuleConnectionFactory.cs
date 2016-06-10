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
    public class ModuleConnectionFactory
    {
        DeviceInformationCollection _devices;
        Dictionary<string, ModuleConnection> _connections = new Dictionary<string, ModuleConnection>();

        public ModuleConnectionFactory()
        {
            try
            {
                Task<DeviceInformationCollection> task = DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort)).AsTask();
                task.Wait();
                _devices = task.Result;

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

        public ModuleConnection GetModuleConnection(ModuleConfigurationModel moduleConfig)
        {
            ModuleConnection connection = null;

            if (_connections.ContainsKey(moduleConfig.ConnectionString))
                return _connections[moduleConfig.ConnectionString];

            try
            {
                var device = _devices.SingleOrDefault(d => d.Id == moduleConfig.ConnectionString);

                if (device == null)
                {
                    string msg = $"Could not find module {moduleConfig.Name} in the device registry. Check the connectionstring or repair the device";
                    //MinimalEventSource.Log.LogWarning(msg);
                    return null;
                }

                if (moduleConfig.ModuleType == ModuleTypes.CABINET)
                    connection = new CabinetModuleConnection(device, moduleConfig.Name);
                else if (moduleConfig.ModuleType == ModuleTypes.SENSORS)
                    connection = new SensorModuleConnection(device, moduleConfig.Name);
                else
                    throw new ArgumentException($"Module type {moduleConfig.ModuleType} is not supported by this device.");

                connection.Init();

                // add to the list of managed connections
                _connections.Add(moduleConfig.ConnectionString, connection);
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogWarning($"Failure initializing connection to module {moduleConfig.Name}: {ex}");
            }

            return connection;
        }
    }
}
