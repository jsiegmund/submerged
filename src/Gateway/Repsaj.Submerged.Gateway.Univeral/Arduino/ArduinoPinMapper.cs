using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Gateway.Universal.Arduino
{
    public static class ArduinoPinMapper
    {
        static KeyValuePair<string, byte>[] pinMapping = new KeyValuePair<string, byte>[]
        {
            new KeyValuePair<string, byte>("D0", 0),
            new KeyValuePair<string, byte>("D1", 1),
            new KeyValuePair<string, byte>("D2", 2),
            new KeyValuePair<string, byte>("D3", 3),
            new KeyValuePair<string, byte>("D4", 4),
            new KeyValuePair<string, byte>("D5", 5),
            new KeyValuePair<string, byte>("D6", 6),
            new KeyValuePair<string, byte>("D7", 7),
            new KeyValuePair<string, byte>("D8", 8),
            new KeyValuePair<string, byte>("D9", 9),
            new KeyValuePair<string, byte>("D10", 10),
            new KeyValuePair<string, byte>("D11", 11),
            new KeyValuePair<string, byte>("D12", 12),
            new KeyValuePair<string, byte>("D13", 13),
            new KeyValuePair<string, byte>("A0", 14),
            new KeyValuePair<string, byte>("A1", 15),
            new KeyValuePair<string, byte>("A2", 16),
            new KeyValuePair<string, byte>("A3", 17),
            new KeyValuePair<string, byte>("A4", 18),
            new KeyValuePair<string, byte>("A5", 19),
            new KeyValuePair<string, byte>("A6", 20),
            new KeyValuePair<string, byte>("A7", 21),
        };

        public static string GetPinName(byte pinNumber)
        {
            KeyValuePair<string, byte>? mapping = pinMapping.SingleOrDefault(m => m.Value == pinNumber);

            if (mapping == null)
                new ArgumentException($"The given pin number '{pinNumber}' was not recognized");

            return ((KeyValuePair<string, byte>)mapping).Key;
        }

        public static byte GetPinNumber(string pinName)
        {
            KeyValuePair<string, byte>? mapping = pinMapping.SingleOrDefault(m => m.Key == pinName);

            if (mapping == null)
                new ArgumentException($"The given pin name '{pinName}' was not recognized");

            return ((KeyValuePair<string, byte>)mapping).Value;
        }
    }
}
