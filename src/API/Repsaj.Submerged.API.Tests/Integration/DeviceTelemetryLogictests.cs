using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autofac;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Infrastructure.Repository;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using System.Threading.Tasks;
using Repsaj.Submerged.API.Tests.Helpers;
using System.Linq;

namespace Repsaj.Submerged.API.Tests.Integration
{
    [TestClass]
    public class DeviceTelemetryLogicTests
    {
        private IContainer _autofacContainer;

        DateTimeOffset _testDateUTC = DateTimeOffset.Parse("2016-06-15T16:52:56.4170000Z");
        int _timeOffsetSeconds = 7200;


        [TestInitialize]
        public void Init()
        {
            var builder = new ContainerBuilder();

            // register configuration provider
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();

            // Repositories
            builder.RegisterType<DeviceTelemetryRepository>().As<IDeviceTelemetryRepository>();
            builder.RegisterType<DeviceTelemetryLogic>().As<IDeviceTelemetryLogic>();

            // build the container and assign it to our test class for use by the individual test methods
            var container = builder.Build();
            _autofacContainer = container;
        }

        [TestMethod]
        public async Task Integration_Logic_LoadLatestDeviceTelemetry()
        {
            IDeviceTelemetryLogic deviceTelemetryLogic = _autofacContainer.Resolve<IDeviceTelemetryLogic>();
            var result = await deviceTelemetryLogic.LoadLatestDeviceTelemetryAsync(TestConfigHelper.DeviceId);

            Assert.AreEqual(result.DeviceId, TestConfigHelper.DeviceId);
            Assert.IsNull(result.LeakDetected);
            Assert.IsTrue(String.IsNullOrEmpty(result.LeakSensors));
            Assert.AreEqual(result.pH, 6.718166666666666);
            Assert.AreEqual(result.Temperature1, 25.5);
            Assert.AreEqual(result.Temperature2, 21);
            Assert.AreEqual(result.EventEnqueuedUTCTime, DateTime.Parse("2016-06-15T17:51:56.3340000Z"));
        }

        [TestMethod]
        public async Task Integration_Logic_LoadDeviceTelemetryReportDataLastThreeHours()
        {
            IDeviceTelemetryLogic deviceTelemetryLogic = _autofacContainer.Resolve<IDeviceTelemetryLogic>();
            var result = await deviceTelemetryLogic.LoadDeviceTelemetryReportDataLastThreeHoursAsync(TestConfigHelper.DeviceId, _testDateUTC.DateTime, _timeOffsetSeconds);

            string[] dataLabels = result.DataLabels.ToArray();
            Assert.AreEqual("15:52", dataLabels[0]);
            Assert.AreEqual("16:22", dataLabels[1]);
            Assert.AreEqual("16:52", dataLabels[2]);
            Assert.AreEqual("17:22", dataLabels[3]);
            Assert.AreEqual("17:52", dataLabels[4]);
            Assert.AreEqual("18:22", dataLabels[5]);

            string[] serieLabels = result.SerieLabels.ToArray();
            Assert.AreEqual("Temperature 1", serieLabels[0]);
            Assert.AreEqual("Temperature 2", serieLabels[1]);
            Assert.AreEqual("pH", serieLabels[2]);

            double?[] temp1Serie = result.DataSeries.First().ToArray();
            Assert.AreEqual(25.5, temp1Serie[0]);
            Assert.AreEqual(25.5, temp1Serie[1]);
            Assert.AreEqual(25.5, temp1Serie[2]);
            Assert.AreEqual(25.5, temp1Serie[3]);
            Assert.AreEqual(25.5, temp1Serie[4]);
            Assert.AreEqual(25.5, temp1Serie[5]);

            double?[] temp2Serie = result.DataSeries.Skip(1).First().ToArray();
            Assert.AreEqual(21.5, temp2Serie[0]);
            Assert.AreEqual(21.5, temp2Serie[1]);
            Assert.AreEqual(21.5, temp2Serie[2]);
            Assert.AreEqual(21.5, temp2Serie[3]);
            Assert.AreEqual(21.5, temp2Serie[4]);
            Assert.AreEqual(21.252777777777776, temp2Serie[5]);

            double?[] pHSerie = result.DataSeries.Skip(2).First().ToArray();
            Assert.AreEqual(6.8622222222222229, pHSerie[0]);
            Assert.AreEqual(6.8272833333333329, pHSerie[1]);
            Assert.AreEqual(6.8086388888888889, pHSerie[2]);
            Assert.AreEqual(6.78928888888889, pHSerie[3]);
            Assert.AreEqual(6.7596888888888893, pHSerie[4]);
            Assert.AreEqual(6.746672222222224, pHSerie[5]);
        }

        [TestMethod]
        public async Task Integration_Logic_LoadDeviceTelemetryReportDataPerDayAsync()
        {
            IDeviceTelemetryLogic deviceTelemetryLogic = _autofacContainer.Resolve<IDeviceTelemetryLogic>();
            var result = await deviceTelemetryLogic.LoadDeviceTelemetryReportDataPerDayAsync(TestConfigHelper.DeviceId, _testDateUTC, _timeOffsetSeconds);

            double?[] expected_avgTemp1 = { 26, 26, 26, 26, 25.987500000000008, 25.598611111111115, 25.5, 25.5, 25.5, 25.5, 25.486111111111111, 25.020833333333332, 25, 25, 25, 25, 25.3, 25.5, 25.5, 25.5, null, null, null, null };
            double?[] expected_avgTemp2 = { 21.5, 21.5, 21.5, 21.5, 21.5, 21.5, 21.5, 21.5, 21.5, 21.086111111111100, 19.847222222222200, 20.1, 20.990277777777700, 21.230555555555500, 21.5, 21.5, 21.554166666666600, 21.5, 21.5, 21.318055555555500, null, null, null, null };
            double?[] expected_avgPH = { 6.8479555555555500, 6.9118194444444400, 6.96901388888889, 7.0240694444444400, 7.0432944444444400, 7.0505972222222200, 7.058072222222220, 7.0660555555555500, 7.0708388888888800, 7.076950000000000, 7.0832388888888900, 7.0931777777777700, 7.1040916666666600, 7.0662055555555500, 6.9957555555555500, 6.9287472222222200, 6.8889055555555500, 6.8399805555555500, 6.7920333333333300, 6.7491722222222200, null, null, null, null };

            Assert.AreEqual(24, result.DataLabels.Count());

            double?[] values_temp1 = result.DataSeries.First().ToArray();
            double?[] values_temp2 = result.DataSeries.Skip(1).First().ToArray();
            double?[] values_pH = result.DataSeries.Skip(2).First().ToArray();

            // assert the number of values is the same
            Assert.AreEqual(expected_avgTemp1.Count(), values_temp1.Count());
            Assert.AreEqual(expected_avgTemp2.Count(), values_temp2.Count());
            Assert.AreEqual(expected_avgPH.Count(), values_pH.Count());

            // assert each array inspecting each value
            for (int i = 0; i < expected_avgTemp1.Count(); i++)
            {
                if (expected_avgTemp1[i] == null)
                    Assert.IsNull(values_temp1[i]);
                else
                    Assert.AreEqual((double)expected_avgTemp1[i], (double)values_temp1[i], 0.0001d, $"Failure on avgTemp1 item {i}");
            }

            for (int i = 0; i < expected_avgTemp2.Count(); i++)
            {
                if (expected_avgTemp2[i] == null)
                    Assert.IsNull(values_temp2[i], $"Failure on avgTemp2 item {i}");
                else
                    Assert.AreEqual((double)expected_avgTemp2[i], (double)values_temp2[i], 0.0001d, $"Failure on avgTemp2 item {i}");
            }
                
            for (int i = 0; i < expected_avgPH.Count(); i++)
            {
                if (expected_avgPH[i] == null)
                    Assert.IsNull(values_pH[i], $"Failure on pH item {i}");
                else
                    Assert.AreEqual((double)expected_avgPH[i], (double)values_pH[i], 0.0001d, $"Failure on avgPH item {i}");
            }
                
        }
    }
}