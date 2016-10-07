using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Helpers;

namespace Repsaj.Submerged.Gateway.Tests.UnitTests
{
    [TestClass]
    public class MiscellaneousTests
    {
        [TestMethod]
        public void Get_SensorReadingToTextPH_Success()
        {
            string result_ph = Converters.ConvertReadingToText(25.58979, SensorTypes.PH);
            Assert.AreEqual("25.59", result_ph);

            string result_ph_null = Converters.ConvertReadingToText(null, SensorTypes.PH);
            Assert.AreEqual("", result_ph_null);
        }

        [TestMethod]
        public void Get_SensorReadingToTextTemperature_Success()
        {
            string result_temp = Converters.ConvertReadingToText(25.58979, SensorTypes.TEMPERATURE);
            Assert.AreEqual("25.6", result_temp);

            string result_temp_null = Converters.ConvertReadingToText(null, SensorTypes.TEMPERATURE);
            Assert.AreEqual("", result_temp_null);
        }

        [TestMethod]
        public void Get_SensorReadingToTextStockFloat_Success()
        {

            string result_float_ok = Converters.ConvertReadingToText(true, SensorTypes.STOCKFLOAT);
            Assert.AreEqual("OK", result_float_ok);

            string result_float_empty = Converters.ConvertReadingToText(false, SensorTypes.STOCKFLOAT);
            Assert.AreEqual("FILL", result_float_empty);

            string result_float_null = Converters.ConvertReadingToText(null, SensorTypes.STOCKFLOAT);
            Assert.AreEqual("", result_float_null);
        }
    }
}
