using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Repsaj.Submerged.GatewayApp.Models;
using Repsaj.Submerged.GatewayApp.Universal.Models;

namespace Repsaj.Submerged.Gateway.Tests
{
    [TestClass]
    public class MiscellaneousTests
    {
        [TestMethod]
        public void Get_SensorReadingToTextPH_Success()
        {
            string result_ph = SensorModel.ReadingToText(25.58979, SensorTypes.PH);
            Assert.AreEqual("25.59", result_ph);

            string result_ph_null = SensorModel.ReadingToText(null, SensorTypes.PH);
            Assert.AreEqual("", result_ph_null);
        }

        [TestMethod]
        public void Get_SensorReadingToTextTemperature_Success()
        {
            string result_temp = SensorModel.ReadingToText(25.58979, SensorTypes.TEMPERATURE);
            Assert.AreEqual("25.6", result_temp);

            string result_temp_null = SensorModel.ReadingToText(null, SensorTypes.TEMPERATURE);
            Assert.AreEqual("", result_temp_null);
        }

        [TestMethod]
        public void Get_SensorReadingToTextStockFloat_Success()
        {

            string result_float_ok = SensorModel.ReadingToText(true, SensorTypes.STOCKFLOAT);
            Assert.AreEqual("OK", result_float_ok);

            string result_float_empty = SensorModel.ReadingToText(false, SensorTypes.STOCKFLOAT);
            Assert.AreEqual("FILL", result_float_empty);

            string result_float_null = SensorModel.ReadingToText(null, SensorTypes.STOCKFLOAT);
            Assert.AreEqual("", result_float_null);
        }
    }
}
