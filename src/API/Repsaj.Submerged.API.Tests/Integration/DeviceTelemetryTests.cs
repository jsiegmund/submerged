using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autofac;
using Repsaj.Submerged.Infrastructure.Repository;
using Repsaj.Submerged.Common.Configurations;
using System.Threading.Tasks;
using Repsaj.Submerged.API.Tests.Helpers;
using System.Linq;

namespace Repsaj.Submerged.API.Tests.Integration
{
    [TestClass]
    public class DeviceTelemetryTests
    {
        private IContainer _autofacContainer;

        DateTime _testDateLocal = DateTime.Parse("2016-06-15T16:52:56.4170000Z");
        DateTime _testDateUTC;


        [TestInitialize]
        public void Init()
        {
            var builder = new ContainerBuilder();

            // register configuration provider
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();

            // Repositories
            builder.RegisterType<DeviceTelemetryRepository>().As<IDeviceTelemetryRepository>();

            // build the container and assign it to our test class for use by the individual test methods
            var container = builder.Build();
            _autofacContainer = container;

            _testDateUTC = _testDateLocal.ToUniversalTime();
        }

        [TestMethod]
        public async Task Integration_LoadLatestDeviceTelemetryAsync()
        {
            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadLatestDeviceTelemetryAsync(TestConfigHelper.DeviceId);

            Assert.AreEqual(result.DeviceId, TestConfigHelper.DeviceId);
            Assert.IsNull(result.LeakDetected);
            Assert.IsTrue(String.IsNullOrEmpty(result.LeakSensors));
            Assert.AreEqual(result.pH, 6.718166666666666);
            Assert.AreEqual(result.Temperature1, 25.5);
            Assert.AreEqual(result.Temperature2, 21);
            Assert.AreEqual(result.EventEnqueuedUTCTime, DateTime.Parse("2016-06-15T17:51:56.3340000Z"));
        }

        [TestMethod]
        public async Task Integration_LoadLatestDeviceTelemetrySummaryAsync()
        {
            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadDeviceTelemetrySummaryAsync(TestConfigHelper.DeviceId, _testDateUTC);

            Assert.AreEqual(result.DeviceId, TestConfigHelper.DeviceId);
            Assert.AreEqual(result.AverageTemp1, 25.486111111111111);
            Assert.AreEqual(result.MinimumTemp1, 25.25);
            Assert.AreEqual(result.MaximumTemp1, 25.5);
            Assert.AreEqual(result.AverageTemp2, 19.847222222222225);
            Assert.AreEqual(result.MinimumTemp2, 19.5);
            Assert.AreEqual(result.MaximumTemp2, 20.5);
            Assert.AreEqual(result.AveragePH, 7.0832388888888937);
            Assert.AreEqual(result.MinimumPH, 7.0621666666666663);
            Assert.AreEqual(result.MaximumPH, 7.1055);
            Assert.AreEqual(result.OutTime, DateTime.Parse("2016-06-15T08:00:00.0000000Z"));
        }

        [TestMethod]
        public async Task Integration_LoadDeviceTelemetryAsync()
        {
            DateTime startUTC = _testDateUTC.AddDays(-1);
            DateTime endUTC = _testDateUTC;

            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadDeviceTelemetryAsync(TestConfigHelper.DeviceId, startUTC, endUTC);

            // sort the results by enqueued time
            result = result.OrderBy(r => r.EventEnqueuedUTCTime);

            var firstRecord = result.First();
            var lastRecord = result.Last();

            Assert.IsTrue(firstRecord.EventEnqueuedUTCTime >= startUTC);
            Assert.IsTrue(lastRecord.EventEnqueuedUTCTime <= endUTC);
        }

        [TestMethod]
        public async Task Integration_LoadDeviceTelemetrySummaryAsync()
        {
            DateTime startUTC = _testDateUTC.AddDays(-1);
            DateTime endUTC = _testDateUTC;

            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadDeviceTelemetrySummaryAsync(TestConfigHelper.DeviceId, startUTC, endUTC);

            // sort the results by enqueued time
            result = result.OrderBy(r => r.OutTime);

            var firstRecord = result.First();
            var lastRecord = result.Last();

            Assert.IsTrue(firstRecord.OutTime >= startUTC);
            Assert.IsTrue(lastRecord.OutTime <= endUTC);
        }
    }
}
