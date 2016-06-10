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
        public static IEnumerable<IEnumerable<double?>> PadTelemetryData(IEnumerable<GroupedTelemetryModel> data, int offset, int count, int modulo = 0)
        {
            List<List<double?>> dataSeries = new List<List<double?>> {
                new List<double?>(),
                new List<double?>(),
                new List<double?>()
            };

            for (int i = offset; dataSeries[0].Count < count; i++)
            {
                int compensated = i;

                if (modulo > 0)
                    compensated = compensated % modulo;

                // try to find a record where the key equals the current index
                var record = data.SingleOrDefault(g => g.Key == compensated);

                // when a record was found; insert the values, otherwise insert null
                if (record != null)
                {
                    dataSeries[0].Add(record.Temperature1);
                    dataSeries[1].Add(record.Temperature2);
                    dataSeries[2].Add(record.pH);
                }
                else
                {
                    dataSeries[0].Add(null);
                    dataSeries[1].Add(null);
                    dataSeries[2].Add(null);
                }
            }

            return dataSeries;
        }

        public static double?[] PadArray(double?[] input, int valueCount)
        {
            List<double?> tempList = new List<double?>(input);

            while (tempList.Count < valueCount)
                tempList.Add(null);

            return tempList.ToArray();
        }
    }
}
