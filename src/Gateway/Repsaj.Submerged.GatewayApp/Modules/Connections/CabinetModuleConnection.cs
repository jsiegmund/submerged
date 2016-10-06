using Microsoft.Maker.Firmata;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.Gateway.Common.Arduino;
using Repsaj.Submerged.GatewayApp.Models;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace Repsaj.Submerged.GatewayApp.Modules.Connections
{
    class CabinetModuleConnection : FirmataModuleConnectionBase, ISensorModule, IRelayModule
    {
        // track information about the relays and sensors connected to this module
        Sensor[] _sensors;
        Relay[] _relays;

        public override string ModuleType
        {
            get
            {
                return ModuleTypeDisplayNames.CABINET;
            }
        }

        byte sensorPin1 = 14;
        byte sensorPin2 = 15;
        byte sensorPin3 = 16;
        byte sensorPin4 = 17;
        byte sensorPin5 = 18;
        byte sensorPin6 = 19;
        byte sensorPin7 = 20;

        byte leakageThreshold = 200;

        public CabinetModuleConnection(DeviceInformation device, string name, Sensor[] sensors, Relay[] relays) : base(device, name)
        {
            this._sensors = sensors;
            this._relays = relays;
        }

        public void SwitchRelay(string relayName, bool high)
        {
            // find the relay by name
            Relay relay = this._relays.SingleOrDefault(r => r.Name == relayName);

            if (relay == null)
                throw new ArgumentException($"No relay was found with name {relayName}");

            try
            {
                foreach (string pinName in relay.PinConfig)
                {
                    byte pin = ArduinoPinMapper.GetPinNumber(pinName);
                    _arduino.pinMode(pin, Microsoft.Maker.RemoteWiring.PinMode.OUTPUT);
                    _arduino.digitalWrite(pin, high ? PinState.LOW : PinState.HIGH);
                }
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError($"Could not switch relay {relayNumber} to state {high} because: {ex}");
                throw ex;       // error needs to be rethrown so the commandprocessor notices the problem
            }
        }

        internal override void _arduino_DeviceReady()
        {
            base._arduino_DeviceReady();
        }

        private void ProcessSensor(ushort value, string sensorName, ref bool leakDetected, ref List<string> triggeredSensors)
        {
            if (value > leakageThreshold)
            {
                leakDetected = true;
                triggeredSensors.Add(sensorName);
            }
        }

        public IEnumerable<SensorTelemetryModel> RequestSensorData()
        {
            if (_adapter.connectionReady())
            {
                _arduino.pinMode(sensorPin1, Microsoft.Maker.RemoteWiring.PinMode.INPUT);
                ushort result1 = _arduino.analogRead("A0");
                _arduino.pinMode(sensorPin2, Microsoft.Maker.RemoteWiring.PinMode.INPUT);
                ushort result2 = _arduino.analogRead("A1");
                _arduino.pinMode(sensorPin3, Microsoft.Maker.RemoteWiring.PinMode.INPUT);
                ushort result3 = _arduino.analogRead("A2");
                _arduino.pinMode(sensorPin4, Microsoft.Maker.RemoteWiring.PinMode.INPUT);
                ushort result4 = _arduino.analogRead("A3");
                _arduino.pinMode(sensorPin5, Microsoft.Maker.RemoteWiring.PinMode.INPUT);
                ushort result5 = _arduino.analogRead("A4");

                _arduino.pinMode(sensorPin6, Microsoft.Maker.RemoteWiring.PinMode.INPUT);
                ushort result6 = _arduino.analogRead("A5");
                _arduino.pinMode(sensorPin7, Microsoft.Maker.RemoteWiring.PinMode.INPUT);
                ushort result7 = _arduino.analogRead("A6");

                bool leakDetected = false;
                List<string> triggeredSensors = new List<string>();

                ProcessSensor(result1, "Sensor 1", ref leakDetected, ref triggeredSensors);
                ProcessSensor(result2, "Sensor 2", ref leakDetected, ref triggeredSensors);
                ProcessSensor(result3, "Sensor 3", ref leakDetected, ref triggeredSensors);
                ProcessSensor(result4, "Sensor 4", ref leakDetected, ref triggeredSensors);
                ProcessSensor(result5, "Sensor 5", ref leakDetected, ref triggeredSensors);

                string leakSensors = String.Join(", ", triggeredSensors);

                List<SensorTelemetryModel> data = new List<SensorTelemetryModel>();
                data.Add(new SensorTelemetryModel("leakDetected", leakDetected));
                data.Add(new SensorTelemetryModel("leakSensors", leakSensors));

                return data;
            }

            return null;
        }

        override internal void _firmata_StringMessageReceived(UwpFirmata caller, StringCallbackEventArgs argv)
        {
            throw new NotImplementedException();
        }
    }
}

