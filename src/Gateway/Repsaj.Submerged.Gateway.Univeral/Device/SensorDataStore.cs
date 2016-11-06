using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using Repsaj.Submerged.GatewayApp.Universal.Helpers;
using Repsaj.Submerged.GatewayApp.Universal.Modules;

namespace Repsaj.Submerged.GatewayApp.Universal.Device
{
    public class SensorDatastore : ISensorDataStore
    {
        ConcurrentDictionary<Tuple<string, string>, FixedSizeQueue<SensorTelemetryModel>> _datastore;

        public SensorDatastore()
        {
            this._datastore = new ConcurrentDictionary<Tuple<string, string>, FixedSizeQueue<SensorTelemetryModel>>();
        }

        /// <summary>
        /// Stores all incoming sensor data in queues per sensor to keep 
        /// track of averages
        /// </summary>
        /// <param name="sensorTelemetry"></param>
        public void ProcessData(ISensorModule module, IEnumerable<SensorTelemetryModel> sensorTelemetry)
        {
            if (sensorTelemetry != null)
            {
                foreach (var telemetryRecord in sensorTelemetry)
                {
                    SaveSensorData(module, telemetryRecord);
                }
            }

            // check for sensors which have a queue but were not present in the collection
            var missingSensorQueues = this._datastore.Where(d => d.Key.Item1 == module.ModuleName && (sensorTelemetry?.Any(s => d.Key.Item2 == s.SensorName) == false))
                                                     .Select(d => new
                                                     {
                                                         SensorName = d.Key.Item2,
                                                         SensorQueue = d.Value
                                                     });

            // store a null for each queue that was not present
            foreach (var missing in missingSensorQueues)
                missing.SensorQueue.Enqueue(new SensorTelemetryModel(missing.SensorName, null));                                                
        }

        private void SaveSensorData(ISensorModule module, SensorTelemetryModel telemetryRecord)
        {
            string sensorName = telemetryRecord.SensorName;

            // ensure there will be a queue for every sensor in the colletion
            if (! _datastore.Keys.Any(k => k.Item1 == module.ModuleName && k.Item2 == sensorName))
            {
                Sensor sensorDefinition = module.Sensors.SingleOrDefault(s => s.Name == sensorName);

                if (sensorDefinition == null)
                    throw new ArgumentException($"Module {module.ModuleName} is not configured for sensor {sensorName}");

                // create a new queue and set the capacity based on the predefined queue capacity
                int queueCapacity = GetQueueCapacityForSensor(sensorDefinition);
                _datastore.TryAdd(new Tuple<string, string>(module.ModuleName, sensorName), new FixedSizeQueue<SensorTelemetryModel>(queueCapacity));
            }
            
            // fetch the queue, store the item and pop off any excess items
            var queue = _datastore.Single(i => i.Key.Item1 == module.ModuleName && i.Key.Item2 == sensorName);
            queue.Value.Enqueue(telemetryRecord);
        }

        private int GetQueueCapacityForSensor(Sensor sensor)
        {
            // for sensors returning numeric values, use a capacity of 6 for a running average
            if (sensor.SensorType == SensorTypes.FLOW ||
                sensor.SensorType == SensorTypes.PH ||
                sensor.SensorType == SensorTypes.TEMPERATURE)
                return 30;

            // for sensors returning booleans or other non-numeric values, use 1 so the datastore will just return the last recorded value
            if (sensor.SensorType == SensorTypes.MOISTURE ||
                sensor.SensorType == SensorTypes.STOCKFLOAT)
                return 1;

            throw new ArgumentException($"The sensor type {sensor.SensorType} was not configured to return a queue capacity.");
        }

        private TelemetryTrendIndication CalculateTrend(IEnumerable<SensorTelemetryModel> telemetry)
        {
            int hasGoneUp = 0;
            int hasGoneDown = 0;
            int hasRemainedEqual = 0;
            SensorTelemetryModel previous = null;

            foreach (var item in telemetry.Where(t => t.Value != null))
            {
                if (previous == null)
                {
                    previous = item;
                    continue;
                }

                // skip when the value inserted is not a double value
                if (!(item.Value is double))
                    continue;

                // increase the correct instance based on comparing current value with previous
                hasGoneUp += Convert.ToDouble(item.Value) > Convert.ToDouble(previous.Value) ? 1 : 0;
                hasGoneDown += Convert.ToDouble(item.Value) < Convert.ToDouble(previous.Value) ? 1 : 0;
                hasRemainedEqual += Convert.ToDouble(item.Value) == Convert.ToDouble(previous.Value) ? 1 : 0;

                previous = item;
            }

            // Exactly one of hasGoneUp and hasGoneDown is true by this point
            double itemCount = telemetry.Count();
            if (hasGoneUp >= (itemCount * 0.6))
                return TelemetryTrendIndication.Increasing;
            else if (hasGoneDown >= (itemCount * 0.6))
                return TelemetryTrendIndication.Decreasing;
            else if (hasRemainedEqual >= (itemCount * 0.5))
                return TelemetryTrendIndication.Equal;
            else
                return TelemetryTrendIndication.Unknown;
        }

        /// <summary>
        /// Returns the average of the sensordata stored in the datastore. Stored null values will not weigh in for the average,
        /// but sensors which only have null values will not be returned.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SensorTelemetryModel> GetData()
        {
            List<SensorTelemetryModel> result = new List<SensorTelemetryModel>();
            int useNValues = 6;

            try
            {
                foreach (var sensorQueue in _datastore)
                {
                    // each sensor stores a number of values. These are used to calculate
                    // the trend and average value, but for the latter we want to use only a subset
                    // of the last n values
                    int numberOfValues = sensorQueue.Value.Count;
                    int skipToLastSix = numberOfValues > useNValues ? numberOfValues - 6 : 0;
                    int takeNumber = numberOfValues < useNValues ? numberOfValues : useNValues;
                    var lastSixValues = sensorQueue.Value.Skip(skipToLastSix).Take(useNValues);

                    // if all of the queue values are null, the sensor should return null
                    if (lastSixValues.All(v => v.Value == null))
                    {
                        result.Add(new SensorTelemetryModel(sensorQueue.Key.Item2, null));
                    }
                    // if the queue length is just one, simply return the value
                    else if (sensorQueue.Value.Count == 1)
                    {
                        result.Add(sensorQueue.Value.First());
                    }
                    // if the queue length is larger then one the sensor values should be numeric and we'll calculate
                    // the running average
                    else
                    {
                        TelemetryTrendIndication trend = CalculateTrend(sensorQueue.Value);
                        double sensorAverage = CalculateAverage(lastSixValues);

                        result.Add(new SensorTelemetryModel(sensorQueue.Key.Item2, sensorAverage, trend));
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cannot calculate stored sensor values due to: {ex}.");
            }

            return result;
        }

        private double CalculateAverage(IEnumerable<SensorTelemetryModel> lastSixValues)
        {
            var values = lastSixValues.Where(v => v.Value != null);

            double sensorSum = values.Sum(v => Convert.ToDouble(v.Value));
            double sensorAverage = sensorSum / (double)values.Count();

            return sensorAverage;
        }
    }
}
