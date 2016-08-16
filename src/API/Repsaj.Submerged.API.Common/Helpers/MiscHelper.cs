using Repsaj.Submerged.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Helpers
{
    public static class MiscHelper
    {
        /// <summary>
        /// Adds additional empty elements to a GroupedTelemetryModel object, to ensure there are enough data elements 
        /// to fill the entire sequence
        /// </summary>
        /// <param name="data">A grouped dataset containing grouped sensor data having unique sensor names per group</param>
        /// <param name="offset">Use to provide an offset for keys to read. Example: if the data provided is grouped by day and starts on Wednesday, first key will be 3 instead of 0 so offset 3 needs to be provided.</param>
        /// <param name="count">The total number of items that should be in the resulting padded dataset</param>
        /// <param name="modulo">Compensates the index to run within a certain range (i.e. 0 - 60 with count 60 and offset 10 should not go up to 70). Does not take offset into account (always starts from 0)</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<double?>> PadTelemetryData(IEnumerable<GroupedTelemetryModel> data, int offset, int count, int modulo = 0)
        {
            Dictionary<string, List<double?>> dataSeries = new Dictionary<string, List<double?>>();

            // if there's no data, return an empty result
            if (data.Count() == 0)
                return dataSeries.Values;

            // create lists for the number of unique sensors in the telemetry model
            string[] sensorNames = data.SelectMany(d => d.SensorData)
                                       .Select(d => d.SensorName)
                                       .Distinct()
                                       .ToArray();

            // if there's no data, return an empty result
            if (sensorNames.Length == 0)
                return dataSeries.Values;

            foreach (string sensorName in sensorNames)
                dataSeries.Add(sensorName, new List<double?>());

            for (int i = offset; i < count + offset; i++)
            {
                int compensated = i;

                if (modulo > 0)
                    compensated = compensated % modulo;

                // try to find a record where the key equals the current index
                var record = data.SingleOrDefault(g => g.Key == compensated);

                // when a record was found; insert the values, otherwise insert null
                if (record != null)
                {
                    foreach (var kvp in dataSeries)
                    {
                        // try to find a matching sensor telemetry element for this serie
                        var elem = record.SensorData.SingleOrDefault(s => s.SensorName == kvp.Key && s.Value is Nullable<double>);

                        if (elem != null)
                            kvp.Value.Add((double?)elem.Value);
                        else
                            kvp.Value.Add(null);
                    }
                }
                else
                {
                    // add a null element in all of the data lists
                    foreach (var list in dataSeries.Values)
                        list.Add(null);
                }
            }

            return dataSeries.Values;
        }

        public static double?[] PadArray(double?[] input, int valueCount)
        {
            List<double?> tempList = new List<double?>(input);

            while (tempList.Count < valueCount)
                tempList.Add(null);

            return tempList.ToArray();
        }

        public static int ProjectHalfHourSegments(DateTime start, DateTime value)
        {
            TimeSpan elapsed = value - start;
            double halfHours = 0;

            // count the full number of hours and multiply that by 2
            halfHours += Math.Floor(elapsed.TotalHours) * 2;
            // add another half hour when there's more than 30 minutes left
            halfHours += elapsed.Minutes >= 30 ? 1 : 0;

            return (int)halfHours;
        }
    }
}
