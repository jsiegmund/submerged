using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Repsaj.Submerged.GatewayApp.Universal.Control.LED;
using Repsaj.Submerged.GatewayApp.Universal.Models.ConfigurationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Gateway.Tests.UnitTests
{
    [TestClass]
    public class LEDUnitTests
    {
        [TestMethod]
        public void LED_RGBWValue_Success()
        {
            LightingPointInTime point = new LightingPointInTime()
            {
                Color = "#e9e9e9",
                White = 233
            };

            RGBWValue value = RGBWHelper.FromLedenetPointInTime(point);

            Assert.IsNotNull(value);
            Assert.AreEqual(233, value.R);
            Assert.AreEqual(233, value.G);
            Assert.AreEqual(233, value.B);
            Assert.AreEqual(233, value.W);
        }

        [TestMethod]
        public void LED_RGBWValue_Success2()
        {
            LightingPointInTime point = new LightingPointInTime()
            {
                Color = "#720031",
                White = 128
            };

            RGBWValue value = RGBWHelper.FromLedenetPointInTime(point);

            Assert.IsNotNull(value);
            Assert.AreEqual(114, value.R);
            Assert.AreEqual(0, value.G);
            Assert.AreEqual(49, value.B);
            Assert.AreEqual(128, value.W);
        }

        [TestMethod]
        public void LED_RGBWValue_Invalid()
        {
            LightingPointInTime point = new LightingPointInTime()
            {
                Color = "invalid",
                White = 233
            };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                RGBWValue value = RGBWHelper.FromLedenetPointInTime(point);
            });
        }

        [TestMethod]
        public void LED_CalculateProgram_Success()
        {
            var points = new List<LightingPointInTime>();

            // 30 minute fade-in at 10AM
            points.Add(new LightingPointInTime()
            {
                Time = 600,
                Color = "#f1f3be",
                FadeIn = 30,
                White = 128
            });

            // 15 minute noon fade-in high output at 2PM
            points.Add(new LightingPointInTime()
            {
                Time = 840,
                Color = "#f1f3be",
                FadeIn = 15,
                White = 255
            });

            // 1 hour fade-out to moonlight at 8PM
            points.Add(new LightingPointInTime()
            {
                Time = 1200,
                Color = "#1a1247",
                FadeIn = 60,
                White = 0
            });

            // 15 minute fade-out at 11PM
            points.Add(new LightingPointInTime()
            {
                Time = 1380,
                Color = "#000000",
                FadeIn = 15,
                White = 0
            });

            var program = RGBWHelper.CalculateProgram(points);

            Assert.AreEqual(1440, program.Length);

            RGBWValue black = new RGBWValue(0, 0, 0, 0);
            RGBWValue morning = new RGBWValue(241, 243, 190, 128);
            RGBWValue noon = new RGBWValue(241, 243, 190, 255);
            RGBWValue moonlight = new RGBWValue(26, 18, 71, 0);

            // check for blackout between 00:00 and 09:30 
            for (int i = 0; i < 570; i++)
                Assert.AreEqual(black, program[i]);

            // check morning color
            Assert.AreEqual(morning, program[10 * 60]);
            // check noon color
            Assert.AreEqual(noon, program[14 * 60]);
            // check moonlight color
            Assert.AreEqual(moonlight, program[20 * 60]);

            // check for blackout between 23:00 and 23:59
            for (int i = 1380; i < 1440; i++)
                Assert.AreEqual(black, program[i]);
        }
    }
}
