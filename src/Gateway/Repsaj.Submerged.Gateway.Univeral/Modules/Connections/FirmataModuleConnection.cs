using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.Firmata;
using Windows.Devices.Enumeration;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Maker.RemoteWiring;
using System.Diagnostics;
using Repsaj.Submerged.Gateway.Universal.Arduino;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.Connections
{
    class FirmataModuleConnection: FirmataModuleConnectionBase, ISensorModule, IRelayModule
    {
        Sensor[] _sensors;
        Relay[] _relays;

        public FirmataModuleConnection(DeviceInformation device, string name, Sensor[] sensors, Relay[] relays) : base(device, name)
        {
            _sensors = sensors;
            _relays = relays;
        }

        internal override void _arduino_DeviceReady()
        {
            InitializePins();

            base._arduino_DeviceReady();
        }

        public override string ModuleType
        {
            get
            {
                return ModuleTypes.FIRMATA;
            }
        }

        private void InitializePins()
        {
            // all sensor pins will be input, some (floats) require internal pullup
            foreach (Sensor sensor in _sensors)
            {
                if (sensor.SensorType == SensorTypes.PH ||
                    sensor.SensorType == SensorTypes.TEMPERATURE ||
                    sensor.SensorType == SensorTypes.MOISTURE || 
                    sensor.SensorType == SensorTypes.FLOW)
                    SetPinMode(sensor.PinConfig, PinMode.INPUT);
                else if (sensor.SensorType == SensorTypes.STOCKFLOAT)
                    SetPinMode(sensor.PinConfig, PinMode.PULLUP);
            }

            // all relay pins will be output pins
            foreach (Relay relay in _relays)
            {
                SetPinMode(relay.PinConfig, PinMode.OUTPUT);
            }
        }

        private void SetPinMode(string[] pins, PinMode pinMode)
        {
            if (pins == null)
                return;

            foreach (string pinName in pins)
            {
                byte pin = ArduinoPinMapper.GetPinNumber(pinName);
                _arduino.pinMode(pin, pinMode);                
            }
        }

        public Task<IEnumerable<SensorTelemetryModel>> RequestSensorData()
        {
            List<SensorTelemetryModel> data = new List<SensorTelemetryModel>();

            foreach (Sensor sensor in _sensors)
            {
                try
                {
                    object sensorValue = GetSensorValue(sensor);
                    data.Add(new SensorTelemetryModel(sensor.Name, sensorValue));
                }   
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception reading sensor value: {ex}");
                    // TODO: error handling
                } 
            }

            IEnumerable<SensorTelemetryModel> result = data;            
            return Task.FromResult(result);
        }

        private object GetSensorValue(Sensor sensor)
        {
            if (sensor.SensorType == SensorTypes.PH)
                return GetPHValue(sensor);
            else if (sensor.SensorType == SensorTypes.STOCKFLOAT)
                return GetStockFloatValue(sensor);
            else if (sensor.SensorType == SensorTypes.TEMPERATURE)
                return GetTemperatureValue(sensor);
            else if (sensor.SensorType == SensorTypes.MOISTURE)
                return GetMoistureValue(sensor);
            else if (sensor.SensorType == SensorTypes.FLOW)
                return GetFlowValue(sensor);
            else
                throw new ArgumentException($"Sensor type '{sensor.SensorType}' is not implemented in the FirmataModule implementation.");
        }

        private double GetTemperatureValue(Sensor sensor)
        {
            if (sensor.PinConfig.Length != 1)
                throw new ArgumentException($"Temperature sensor '{sensor.Name}' does not have exactly one pin configured. Check your configuration.");

            string pinName = sensor.PinConfig[0];
            if (!pinName.StartsWith("A"))
                throw new ArgumentException($"Temperature sensor '{sensor.Name}' needs to be connected to an analog input instead of '{pinName}'.");

            ushort value = GetAnalogPinValue(pinName);
            return value;
        }

        private double GetPHValue(Sensor sensor)
        {
            throw new NotImplementedException();
        }

        private bool GetMoistureValue(Sensor sensor)
        {
            if (sensor.PinConfig.Length != 1)
                throw new ArgumentException($"Moisure sensor '{sensor.Name}' does not have exactly one pin configured. Check your configuration.");

            string pinName = sensor.PinConfig[0];
            if (!pinName.StartsWith("A"))
                throw new ArgumentException($"Moisture sensor '{sensor.Name}' needs to be connected to an analog input instead of '{pinName}'.");

            ushort value = GetAnalogPinValue(sensor.PinConfig[0]);

            Debug.WriteLine($"Moisture sensor {sensor.Name} reads: {value}");

            // when a value larger than 50 is detected, there's moisture involved
            return value > 50;
        }

        private ushort GetFlowValue(Sensor sensor)
        {
            if (sensor.PinConfig.Length != 1)
                throw new ArgumentException($"Flow sensor '{sensor.Name}' does not have exactly one pin configured. Check your configuration.");

            string pinName = sensor.PinConfig[0];
            if (!pinName.StartsWith("D"))
                throw new ArgumentException($"Flow sensor '{sensor.Name}' needs to be connected to a digital input instead of '{pinName}'.");

            // TODO: we cannot just get an analog pin value here, flow sensor needs to use interrupts 
            ushort value = GetAnalogPinValue(sensor.PinConfig[0]);

            Debug.WriteLine($"Flow sensor {sensor.Name} reads: {value}");

            // when a value larger than 50 is detected, there's moisture involved
            return value;
        }

        private bool GetStockFloatValue(Sensor sensor)
        {
            if (sensor.PinConfig.Length != 1)
                throw new ArgumentException($"Stock float sensor '{sensor.Name}' does not have exactly one pin configured. Check your configuration.");

            string pinName = sensor.PinConfig[0];
            if (!pinName.StartsWith("D"))
                throw new ArgumentException($"Stock float sensor '{sensor.Name}' needs to be connected to an digital input instead of '{pinName}'.");

            PinState value = GetDigitalPinValue(sensor.PinConfig[0]);

            // this is a switch sensor, return bool instead of value
            return value == PinState.HIGH;
        }

        private ushort GetAnalogPinValue(string pinName)
        {
            byte pinNumber = ArduinoPinMapper.GetPinNumber(pinName);
            return _arduino.analogRead(pinName);
        }

        private PinState GetDigitalPinValue(string pinName)
        {
            byte pinNumber = ArduinoPinMapper.GetPinNumber(pinName);
            return _arduino.digitalRead(pinNumber);
        }

        internal override void _firmata_StringMessageReceived(UwpFirmata caller, StringCallbackEventArgs argv)
        {
            throw new NotImplementedException();
        }

        public void SwitchRelay(string name, bool high)
        {
            throw new NotImplementedException();
        }
    }
}
