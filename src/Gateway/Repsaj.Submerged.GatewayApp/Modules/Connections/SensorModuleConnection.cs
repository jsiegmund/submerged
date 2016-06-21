using Microsoft.Maker.Firmata;
using Microsoft.Maker.Serial;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
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

    class SensorModuleConnection : ModuleConnectionBase
    {
        Queue<Measurement> _measurementQueue = new Queue<Measurement>();

        private static object _queueLock = new object();

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
        dynamic _data = null;

        override public dynamic RequestArduinoData()
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
                    return _data;
                }
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError("Could not translate received bytes from Arduino into telemetry message: " + ex.ToString());
            }

            return null;
        }

        override internal void _firmata_StringMessageReceived(UwpFirmata caller, StringCallbackEventArgs argv)
        {
            try
            {
                var content = argv.getString();
                dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                if (jsonObject == null)
                    return;

                // lock the part where we use the queue to prevent any simulatenous access to it
                lock (_queueLock)
                {
                    Measurement measurement = new Measurement()
                    {
                        temperature1 = jsonObject.temp1,
                        temperature2 = jsonObject.temp2,
                        pH = jsonObject.pH
                    };

                    _measurementQueue.Enqueue(measurement);
                    if (_measurementQueue.Count > 6)
                        _measurementQueue.Dequeue();


                    dynamic data = new ExpandoObject();
                    data.temperature1 = _measurementQueue.Sum(s => s.temperature1) / _measurementQueue.Count;
                    data.temperature2 = _measurementQueue.Sum(s => s.temperature2) / _measurementQueue.Count;
                    data.pH = _measurementQueue.Sum(s => s.pH) / _measurementQueue.Count;
                    _data = data;
                }

                _waitHandle.Set();
            }
            catch (Exception ex)
            {
                //MinimalEventSource.Log.LogError("Could not translate received bytes from Arduino into telemetry message: " + ex.ToString());
            }
        }
    }
}
