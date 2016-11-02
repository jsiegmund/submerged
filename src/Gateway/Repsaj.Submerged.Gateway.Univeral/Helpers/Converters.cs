using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Helpers
{
    public static class Converters
    {
        public static string ConvertReadingToText(object reading, string sensorType)
        {
            string formattedText = "";

            if (reading != null)
            {
                Type readingType = reading.GetType();

                if (readingType == typeof(Double) ||
                    readingType == typeof(float) ||
                    readingType == typeof(ushort))
                {
                    if (sensorType == SensorTypes.TEMPERATURE)
                        formattedText = String.Format("{0:0.0}", reading);
                    else if (sensorType == SensorTypes.PH)
                        formattedText = String.Format("{0:0.00}", reading);
                    else if (sensorType == SensorTypes.FLOW)
                        formattedText = String.Format("{0:0}", reading);
                    else
                        formattedText = String.Format("{0:0.0}", reading);
                }
                else if (reading is bool? && sensorType == SensorTypes.STOCKFLOAT)
                {
                    bool? readingAsBool = (bool?)reading;
                    if (readingAsBool.HasValue)
                        formattedText = readingAsBool.Value ? "FILL" : "OK";
                }
                else
                    formattedText = reading?.ToString();
            }

            return formattedText;
        }
    }
}
