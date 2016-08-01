using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Helpers
{
    static class Randomizer
    {
        private static Random _rand = new Random();

        public static double GetRandomDouble(double baseValue, double rangeValue)
        {
            return baseValue + _rand.NextDouble() * rangeValue - (rangeValue / 2);
        }

        public static bool GetRandomBool()
        {
            return _rand.NextDouble() > 0.5;
        }

        internal static double GetRandomInt(int baseValue, int rangeValue)
        {
            return (int)GetRandomDouble(baseValue, rangeValue);
        }
    }
}
