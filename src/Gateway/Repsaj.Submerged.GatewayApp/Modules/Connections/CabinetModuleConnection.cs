using Microsoft.Maker.Firmata;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace Repsaj.Submerged.GatewayApp.Modules.Connection
{
    public class CabinetModuleConnection : ModuleConnectionBase
    {
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

        byte relayPin1 = 2;
        byte relayPin2 = 3;
        byte relayPin3 = 4;
        byte relayPin4 = 5;

        byte leakageThreshold = 200;

        public CabinetModuleConnection(DeviceInformation device, string name) : base(device, name)
        {

        }

        public void SwitchRelay(int relayNumber, bool high)
        {
            try
            {
                // the PinState of the relays is not intuitive because the default state of the relay is ON, 
                // so for switching the relay OFF we need to set the pin HIGH and vice versa
                switch (relayNumber)
                {
                    case 1:
                        _arduino.pinMode(relayPin1, Microsoft.Maker.RemoteWiring.PinMode.OUTPUT);
                        _arduino.digitalWrite(relayPin1, high ? PinState.LOW : PinState.HIGH);
                        break;
                    case 2:
                        _arduino.pinMode(relayPin2, Microsoft.Maker.RemoteWiring.PinMode.OUTPUT);
                        _arduino.digitalWrite(relayPin2, high ? PinState.LOW : PinState.HIGH);
                        break;
                    case 3:
                        _arduino.pinMode(relayPin3, Microsoft.Maker.RemoteWiring.PinMode.OUTPUT);
                        _arduino.digitalWrite(relayPin3, high ? PinState.LOW : PinState.HIGH);
                        break;
                    case 4:
                        _arduino.pinMode(relayPin4, Microsoft.Maker.RemoteWiring.PinMode.OUTPUT);
                        _arduino.digitalWrite(relayPin4, high ? PinState.LOW : PinState.HIGH);
                        break;
                }

                string pinState = high ? "HIGH" : "LOW";
                //MinimalEventSource.Log.LogInfo($"Switched relay {relayNumber} to state {pinState} because of cloud command");
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

        override public dynamic RequestArduinoData()
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

                bool leakDetected = false;
                List<string> triggeredSensors = new List<string>();

                ProcessSensor(result1, "Sensor 1", ref leakDetected, ref triggeredSensors);
                ProcessSensor(result2, "Sensor 2", ref leakDetected, ref triggeredSensors);
                ProcessSensor(result3, "Sensor 3", ref leakDetected, ref triggeredSensors);
                ProcessSensor(result4, "Sensor 4", ref leakDetected, ref triggeredSensors);
                ProcessSensor(result5, "Sensor 5", ref leakDetected, ref triggeredSensors);

                dynamic data = new System.Dynamic.ExpandoObject();
                data.leakDetected = leakDetected;
                data.leakSensors = String.Join(", ", triggeredSensors);

                Debug.WriteLine($"Leak detection: {data.leakDetected}, sensors: {data.leakSensors}");

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

