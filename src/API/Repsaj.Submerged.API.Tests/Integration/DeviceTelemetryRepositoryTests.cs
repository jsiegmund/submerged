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
    public class DeviceTelemetryRepositoryTests
    {
        private IContainer _autofacContainer;
        DateTimeOffset _testDateUTC = DateTimeOffset.Parse("2016-08-09T18:58:56.4170000Z");


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
        }

        [TestMethod]
        public async Task Integration_Repository_LoadLatestDeviceTelemetryAsync()
        {
            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadLatestDeviceTelemetryAsync(TestConfigHelper.DeviceId);

            Assert.AreEqual(result.DeviceId, TestConfigHelper.DeviceId);
            Assert.AreEqual(result.EventEnqueuedUTCTime, DateTime.Parse("2016-06-15T17:51:56.3340000Z"));
            // TODO: redo tests to match new telemetry models
            //Assert.IsNull(result.LeakDetected);
            //Assert.IsTrue(String.IsNullOrEmpty(result.LeakSensors));
            //Assert.AreEqual(result.pH, 6.718166666666666);
            //Assert.AreEqual(result.Temperature1, 25.5);
            //Assert.AreEqual(result.Temperature2, 21);
        }

        [TestMethod]
        public async Task Integration_Repository_LoadLatestDeviceTelemetrySummaryAsync()
        {
            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadDeviceTelemetrySummaryAsync(TestConfigHelper.DeviceId, _testDateUTC);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.DeviceId, TestConfigHelper.DeviceId);
            Assert.AreEqual(result.OutTime, DateTime.Parse("2016-06-15T17:00:00.0000000Z"));

            // TODO: redo tests to match new telemetry models
            //Assert.AreEqual(result.AverageTemp1, 25.5);
            //Assert.AreEqual(result.MinimumTemp1, 25.5);
            //Assert.AreEqual(result.MaximumTemp1, 25.5);
            //Assert.AreEqual(result.AverageTemp2, 21.318055555555556);
            //Assert.AreEqual(result.MinimumTemp2, 21);
            //Assert.AreEqual(result.MaximumTemp2, 21.5);
            //Assert.AreEqual(result.AveragePH, 6.7491722222222217);
            //Assert.AreEqual(result.MinimumPH, 6.7146666666666661);
            //Assert.AreEqual(result.MaximumPH, 6.7834999999999992);
        }

        [TestMethod]
        public async Task Integration_Repository_LoadDeviceTelemetryAsync()
        {
            DateTimeOffset startUTC = _testDateUTC.AddDays(-1);
            DateTimeOffset endUTC = _testDateUTC;

            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadDeviceTelemetryAsync(TestConfigHelper.DeviceId, startUTC, endUTC);

            Assert.IsFalse(result.Count() == 0);

            // sort the results by enqueued time
            result = result.OrderBy(r => r.EventEnqueuedUTCTime);

            var firstRecord = result.First();
            var lastRecord = result.Last();

            Assert.IsTrue(firstRecord.EventEnqueuedUTCTime >= startUTC);
            Assert.IsTrue(lastRecord.EventEnqueuedUTCTime <= endUTC);
        }

        [TestMethod]
        public async Task Integration_Repository_LoadDeviceTelemetrySummaryAsync()
        {
            DateTimeOffset startUTC = _testDateUTC.AddDays(-1);
            DateTimeOffset endUTC = _testDateUTC;

            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadDeviceTelemetrySummaryAsync(TestConfigHelper.DeviceId, startUTC, endUTC);

            // sort the results by enqueued time
            result = result.OrderBy(r => r.OutTime);

            var firstRecord = result.First();
            var lastRecord = result.Last();

            Assert.IsTrue(firstRecord.OutTime >= startUTC);
            Assert.IsTrue(lastRecord.OutTime <= endUTC);
        }

        [TestMethod]
        public async Task Integration_Repository_LoadDeviceTelemetrySummaryThreeHours()
        {
            DateTimeOffset windowUTCStart = _testDateUTC.AddHours(-3);
            DateTimeOffset windowUTCEnd = _testDateUTC;

            IDeviceTelemetryRepository repository = _autofacContainer.Resolve<IDeviceTelemetryRepository>();
            var result = await repository.LoadDeviceTelemetryAsync(TestConfigHelper.DeviceId, windowUTCStart, windowUTCEnd);

            Assert.AreEqual(result.Count(), 180);
        }
    }
}
