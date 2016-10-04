using Microsoft.Maker.Serial;
using Repsaj.Submerged.GatewayApp.Modules.Connections;
using Repsaj.Submerged.GatewayApp.Modules.Simulated;
using Repsaj.Submerged.GatewayApp.Universal.Exceptions;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Models.ConfigurationModels;
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

        public IModuleConnection GetModuleConnection(Module module, Sensor[] sensors, Relay[] relays)
        {
            IModuleConnection connection = null;

            if (_connections.ContainsKey(module.Name))
                return _connections[module.Name];

            try
            {
                var simulated = module.ConnectionString == "simulated";
                var device = _devices.SingleOrDefault(d => d.Id.Contains(module.ConnectionString));

                if (!simulated)
                    connection = CreateConnection(module, device, sensors, relays);
                else
                    connection = CreateSimulatedConnection(module, sensors, relays);

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

        private IModuleConnection CreateSimulatedConnection(Module module, Sensor[] sensors, Relay[] relays)
        {
            if (module.ModuleType == ModuleTypes.CABINET)
                return new SimulatedCabinetModuleConnection(module.Name);
            else if (module.ModuleType == ModuleTypes.SENSORS)
                return new SimulatedSensorModuleConnection(module.Name);
            else if (module.ModuleType == ModuleTypes.FIRMATA)
                return new SimulatedFirmataModuleConnection(module.Name, sensors, relays);
            else if (module.ModuleType == ModuleTypes.LEDENET)
                return new SimulatedLedenetModuleConnection(module.Name);
            else
                throw new ArgumentException($"Module type {module.ModuleType} is not supported by this device.");
        }

        private IModuleConnection CreateConnection(Module module, DeviceInformation device, Sensor[] sensors, Relay[] relays)
        {
            if (module.ModuleType == ModuleTypes.CABINET)
                return new CabinetModuleConnection(device, module.Name, sensors, relays);
            else if (module.ModuleType == ModuleTypes.SENSORS)
                return new SensorModuleConnection(device, module.Name);
            else if (module.ModuleType == ModuleTypes.FIRMATA)
                return new FirmataModuleConnection(device, module.Name, sensors, relays);
            else if (module.ModuleType == ModuleTypes.LEDENET)
                return new LedenetModuleConnection(module.Name, module.Configuration.ToObject<LedenetModuleConfiguration>());
            else
                throw new ArgumentException($"Module type {module.ModuleType} is not supported by this device.");
        }
    }
}
