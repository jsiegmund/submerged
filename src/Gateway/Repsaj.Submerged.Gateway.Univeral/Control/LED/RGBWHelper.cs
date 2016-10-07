using Repsaj.Submerged.GatewayApp.Universal.Models.ConfigurationModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Control.LED
{
    public class RGBWHelper
    {
        public static RGBWValue[] CalculateProgram(IEnumerable<LightingPointInTime> points)
        {
            // This method calculates the program by calculating the color as expected
            // for every individual minute. It stores the program as a simple array 
            // of size 1440, one entry for every minute.
            var result = new RGBWValue[1440];

            // we start with the levels all being 0
            RGBWValue activeValue = new RGBWValue();

            // we'll calcute the levels for each minute of the day
            for (int time = 0; time < 24 * 60; time++)
            {
                // find a point of which the time or fade time is in range
                var pointsInRange = points.Where(p => p.Time == time || (((p.Time - p.FadeIn) <= time) && (p.Time >= time)))
                                          .OrderByDescending(p => p.Time);

                // when there are no points in range, we will simply copy the current value
                if (pointsInRange.Count() == 0)
                {
                    result[time] = activeValue;
                    continue;
                }

                // if there's exactly one point, see whether it has the exact same time
                if (pointsInRange.Count() >= 1)
                {
                    LightingPointInTime pointInRange = pointsInRange.First();

                    if (pointInRange.Time == time)
                    {
                        // set the current value to this new point and update the program
                        activeValue = FromLedenetPointInTime(pointInRange);
                        result[time] = activeValue;
                    }
                    else
                    {
                        // calculate the fading value between the previous color and the next
                        RGBWValue fadingValue = CalculateRGBWValue(activeValue, pointInRange, time);
                        result[time] = fadingValue;
                    }
                }
            }

            return result;
        }

        public static RGBWValue FromLedenetPointInTime(LightingPointInTime point)
        {
            RGBWValue result = new RGBWValue();

            if (point == null)
                return result;

            // check the color is in a valid format which should be #000000
            Regex hexColor = new Regex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
            if (hexColor.IsMatch(point.Color))
            {
                result.R = int.Parse(point.Color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                result.G = int.Parse(point.Color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                result.B = int.Parse(point.Color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                throw new ArgumentException($"The supplied color ({point.Color} is not a valid hex color.");
            }

            result.W = point.White;

            return result;
        }

        /// <summary>
        /// Calculates the RGBWValue of the current time passed in, based on the 
        /// </summary>
        /// <param name="pointInTime"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static RGBWValue CalculateRGBWValue(RGBWValue previous, LightingPointInTime nextPoint, double time)
        {
            RGBWValue result = new RGBWValue();

            // we first need to calculate how far along the fade we are
            double startFade = nextPoint.Time - nextPoint.FadeIn;
            double endFade = nextPoint.Time;
            double progress = ((time - startFade) / (endFade - startFade)) * 100;

            RGBWValue nextColor = FromLedenetPointInTime(nextPoint);

            // now we need to find the correct color value based on the previous 
            // color, the next color and the progress number
            result.R = CalculateFadingColor(previous.R, nextColor.R, progress);
            result.G = CalculateFadingColor(previous.R, nextColor.G, progress);
            result.B = CalculateFadingColor(previous.R, nextColor.B, progress);
            result.W = CalculateFadingColor(previous.R, nextColor.W, progress);

            return result;
        }

        private static int CalculateFadingColor(int previous, int next, double progress)
        {
            if (progress < 0 || progress > 100)
                throw new ArgumentException($"Progress should be between 0 and 100, {progress} is not.");

            return (int)(previous + ((next - previous) * (progress / 100)));
        }
    }
}
