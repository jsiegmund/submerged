using Microsoft.Maker.Firmata;
using Microsoft.Maker.Serial;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.Gateway.Common.Log;
using Repsaj.Submerged.GatewayApp.Models;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace Repsaj.Submerged.GatewayApp.Modules.Connections
{
    struct Measurement
    {
        public double temperature1;
        public double temperature2;
        public double pH;
    }

    class SensorModuleConnection : FirmataModuleConnectionBase, ISensorModule
    {
        ConcurrentQueue<Measurement> _measurementQueue = new ConcurrentQueue<Measurement>();

        public override string ModuleType
        {
            get
            {
                return ModuleTypeDisplayNames.SENSORS;
            }
        }

        public SensorModuleConnection(DeviceInformation device, string name) : base(device, name)
        { }

        EventWaitHandle _waitHandle = new AutoResetEvent(false);
        List<SensorTelemetryModel> _sensorData = new List<SensorTelemetryModel>();

        public IEnumerable<SensorTelemetryModel> RequestSensorData()
        {
            try
            {
                if (_adapter.connectionReady())
                {
                    _waitHandle.Reset();

                    lock (ConnectionLock)
                    {
                        byte NEPTUNE_DATA = 0x44;
                        _firmata.sendSysex(NEPTUNE_DATA, new byte[] { }.AsBuffer());
                        _firmata.flush();
                    }

                    _waitHandle.WaitOne();
                    return _sensorData;
                }
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error("Exception occurred requesting sensor data from the Sensor Module: " + ex.ToString());
                //MinimalEventSource.Log.LogError("Could not translate received bytes from Arduino into telemetry message: " + ex.ToString());
            }

            return null;
        }

        override internal void _firmata_StringMessageReceived(UwpFirmata caller, StringCallbackEventArgs argv)
        {
            var content = argv.getString();

            try
            {
                dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                if (jsonObject == null)
                    return;

                // lock the part where we use the queue to prevent any simulatenous access to it
                _sensorData.Clear();

                Measurement measurement = new Measurement()
                {
                    temperature1 = jsonObject.temp1,
                    temperature2 = jsonObject.temp2,
                    pH = jsonObject.pH
                };

                Measurement deletedMeasurement;

                _measurementQueue.Enqueue(measurement);

                // ensure there's a maximum of 6 items in the queue at any given time
                if (_measurementQueue.Count > 6)
                    _measurementQueue.TryDequeue(out deletedMeasurement);

                var temp1 = _measurementQueue.Sum(s => s.temperature1) / _measurementQueue.Count;
                var temp2 = _measurementQueue.Sum(s => s.temperature2) / _measurementQueue.Count;
                var pH = _measurementQueue.Sum(s => s.pH) / _measurementQueue.Count;

                _sensorData.Add(new SensorTelemetryModel("temperature1", temp1));
                _sensorData.Add(new SensorTelemetryModel("temperature2", temp2));
                _sensorData.Add(new SensorTelemetryModel("pH", pH));

                _waitHandle.Set();
            }
            catch(Newtonsoft.Json.JsonReaderException)
            {
                LogEventSource.Log.Error("Could not parse JSON message received from Sensor Module: " + content);
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error("Exception was caught processing firmata message in Sensor Module: " + ex.ToString());
            }
        }
    }
}
