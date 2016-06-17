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
            Assert.AreEqual(dataLabels[0], "15:52");
            Assert.AreEqual(dataLabels[1], "16:22");
            Assert.AreEqual(dataLabels[2], "16:52");
            Assert.AreEqual(dataLabels[3], "17:22");
            Assert.AreEqual(dataLabels[4], "17:52");
            Assert.AreEqual(dataLabels[5], "18:22");

            string[] serieLabels = result.SerieLabels.ToArray();
            Assert.AreEqual(serieLabels[0], "Temperature 1");
            Assert.AreEqual(serieLabels[1], "Temperature 2");
            Assert.AreEqual(serieLabels[2], "pH");

            double?[] temp1Serie = result.DataSeries.First().ToArray();
            double?[] temp2Serie = result.DataSeries.Skip(1).First().ToArray();
            double?[] pHSerie = result.DataSeries.Skip(2).First().ToArray();
        }
    }
}