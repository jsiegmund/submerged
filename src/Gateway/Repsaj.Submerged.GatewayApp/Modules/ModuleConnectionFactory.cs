﻿using Microsoft.Maker.Serial;
using Repsaj.Submerged.GatewayApp.Modules.Connections;
using Repsaj.Submerged.GatewayApp.Modules.Simulated;
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

namespace Repsaj.Submerged.GatewayApp.Modules
{
    public class ModuleConnectionFactory : IModuleConnectionFactory
    {
        DeviceInformationCollection _devices;
        Dictionary<string, IModuleConnection> _connections = new Dictionary<string, IModuleConnection>();

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

        public IModuleConnection GetModuleConnection(string moduleName)
        {
            return _connections[moduleName];
        }

        public IModuleConnection GetModuleConnection(Module module)
        {
            IModuleConnection connection = null;

            if (_connections.ContainsKey(module.Name))
                return _connections[module.Name];

            try
            {
                var simulated = module.ConnectionString == "simulated";
                var device = _devices.SingleOrDefault(d => d.Id == module.ConnectionString);

                if (device == null && !simulated)
                {
                    string msg = $"Could not find module {module.Name} in the device registry. Check the connectionstring or repair the device";
                    //MinimalEventSource.Log.LogWarning(msg);
                    return null;
                }

                if (!simulated)
                    connection = CreateConnection(module, device);
                else
                    connection = CreateSimulatedConnection(module);

                connection.Init();

                // add to the list of managed connections
                _connections.Add(module.Name, connection);
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogWarning($"Failure initializing connection to module {moduleConfig.Name}: {ex}");
            }

            return connection;
        }

        private IModuleConnection CreateSimulatedConnection(Module module)
        {
            if (module.ModuleType == ModuleTypes.CABINET)
                return new SimulatedCabinetModuleConnection(module.Name);
            else if (module.ModuleType == ModuleTypes.SENSORS)
                return new SimulatedSensorModuleConnection(module.Name);
            else
                throw new ArgumentException($"Module type {module.ModuleType} is not supported by this device.");
        }

        private IModuleConnection CreateConnection(Module module, DeviceInformation device)
        {
            if (module.ModuleType == ModuleTypes.CABINET)
                return new CabinetModuleConnection(device, module.Name);
            else if (module.ModuleType == ModuleTypes.SENSORS)
                return new SensorModuleConnection(device, module.Name);
            else
                throw new ArgumentException($"Module type {module.ModuleType} is not supported by this device.");
        }
    }
}