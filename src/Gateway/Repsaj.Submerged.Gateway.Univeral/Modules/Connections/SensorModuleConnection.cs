﻿using Microsoft.Maker.Firmata;
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
using Repsaj.Submerged.GatewayApp.Universal.Commands;

namespace Repsaj.Submerged.GatewayApp.Universal.Modules.Connections
{
    struct Measurement
    {
        public double? temperature1;
        public double? temperature2;
        public double? pH;
    }

    class SensorModuleConnection : FirmataModuleConnectionBase, ISensorModule
    {
        Measurement _measurement;

        public IEnumerable<Sensor> Sensors
        {
            get { return _sensors; }
        }

        SemaphoreSlim _measurementSemaphore = new SemaphoreSlim(1, 1);
        Sensor[] _sensors;
        Relay[] _relays;

        public override string ModuleType
        {
            get
            {
                return ModuleTypeDisplayNames.SENSORS;
            }
        }

        public SensorModuleConnection(DeviceInformation device, string name, Sensor[] sensors, Relay[] relays) : base(device, name)
        {
            // create a new completion source which will be set by the firmata response event
            //          _sensorData = new ConcurrentBag<SensorTelemetryModel>();
            this._sensors = sensors;
            this._relays = relays;

            this._measurement = new Measurement();
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

            List<SensorTelemetryModel> result = new List<SensorTelemetryModel>();

            // build the result, don't add records for null measurements 
            _measurementSemaphore.Wait();
            if (this._measurement.temperature1 != null)
                result.Add(new SensorTelemetryModel("temperature1", this._measurement.temperature1));
            if (this._measurement.temperature2 != null)
                result.Add(new SensorTelemetryModel("temperature2", this._measurement.temperature2));
            if (this._measurement.pH != null)
                result.Add(new SensorTelemetryModel("pH", this._measurement.pH));
            _measurementSemaphore.Release();

            return result;
        }

        override internal void _firmata_StringMessageReceived(UwpFirmata caller, StringCallbackEventArgs argv)
        {
            var content = argv.getString();

            try
            {
                _measurementSemaphore.Wait();
                
                // reset the measurement object to null
                this._measurement.temperature1 = null;
                this._measurement.temperature2 = null;
                this._measurement.pH = null;

                // deserialize the json into a dynamic object, return when failed
                dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                if (jsonObject == null)
                    return;

                // copy the values onto the measurement object
                this._measurement.temperature1 = jsonObject.temp1;
                this._measurement.temperature2 = jsonObject.temp2;
                this._measurement.pH = jsonObject.pH;
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
            finally
            {
                _measurementSemaphore.Release();
            }
        }

        public override Task ProcessCommand(dynamic command)
        {
            throw new NotImplementedException();
        }
    }
}