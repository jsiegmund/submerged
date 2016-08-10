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
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="modulo"></param>
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

            foreach (string sensorName in sensorNames)
                dataSeries.Add(sensorName, new List<double?>());

            for (int i = offset; dataSeries.First().Value.Count < count; i++)
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
