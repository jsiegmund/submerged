using Microsoft.Maker.Firmata;
using Microsoft.Maker.Serial;
using Newtonsoft.Json.Linq;
using Repsaj.Submerged.Gateway.Common.Log;
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
using Repsaj.Submerged.GatewayApp.Universal.Extensions;
using Newtonsoft.Json;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.Connections
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
//        ConcurrentBag<SensorTelemetryModel> _sensorData;

        public override string ModuleType
        {
            get
            {
                return ModuleTypeDisplayNames.SENSORS;
            }
        }

        public SensorModuleConnection(DeviceInformation device, string name) : base(device, name)
        {
            // create a new completion source which will be set by the firmata response event
  //          _sensorData = new ConcurrentBag<SensorTelemetryModel>();
        }

        public async Task<IEnumerable<SensorTelemetryModel>> RequestSensorData()
        {
            try
            {
                if (!_adapter.connectionReady())
                {
                    SetModuleStatus(ModuleConnectionStatus.Disconnected);
                    return null;
                }

                Debug.WriteLine("Awaiting connection lock");
                // wait for the connection lock and for the firmata response
                _connectionLock.Wait();

                Debug.WriteLine("Sending request");
                // send out the request
                byte NEPTUNE_DATA = 0x44;
                _firmata.sendSysex(NEPTUNE_DATA, new byte[] { }.AsBuffer());
                _firmata.flush();

                Debug.WriteLine("Awaiting firmata response for thirty seconds");
                // Wait for 30 seconds for the response to come in
                await Task.Delay(new TimeSpan(0, 0, 5));
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error("Exception occurred requesting sensor data from the Sensor Module: " + ex.ToString());
                SetModuleStatus(ModuleConnectionStatus.Disconnected);
            }
            finally
            {
                Debug.WriteLine("Releasing connection lock");
                _connectionLock.Release();
            }

            if (_measurementQueue.Count > 0)
            {
                List<SensorTelemetryModel> result = new List<SensorTelemetryModel>();

                var temp1 = _measurementQueue.Sum(s => s.temperature1) / _measurementQueue.Count;
                var temp2 = _measurementQueue.Sum(s => s.temperature2) / _measurementQueue.Count;
                var pH = _measurementQueue.Sum(s => s.pH) / _measurementQueue.Count;

                result.Add(new SensorTelemetryModel("temperature1", temp1));
                result.Add(new SensorTelemetryModel("temperature2", temp2));
                result.Add(new SensorTelemetryModel("pH", pH));

                return result;
            }
            else
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

                Measurement measurement = new Measurement()
                {
                    temperature1 = jsonObject.temp1,
                    temperature2 = jsonObject.temp2,
                    pH = jsonObject.pH
                };

                _measurementQueue.Enqueue(measurement);

                // ensure there's a maximum of 6 items in the queue at any given time
                Measurement deletedMeasurement;
                while (_measurementQueue.Count > 6)
                    _measurementQueue.TryDequeue(out deletedMeasurement);
            }
            catch (Exception ex) when (ex is JsonSerializationException | ex is JsonReaderException)
            {
                // received response but it was invalid, just warn
                LogEventSource.Log.Warn("Could not parse JSON message received from Sensor Module: " + content);
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error("Exception was caught processing firmata message in Sensor Module: " + ex.ToString());
            }
        }
    }
}
