using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autofac.Extras.Moq;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using System.Threading.Tasks;
using Repsaj.Submerged.Infrastructure.Models;
using Repsaj.Submerged.API.Tests.Helpers;

namespace Repsaj.Submerged.API.Tests.UnitTests
{
    [TestClass]
    public class DeviceTelemetryLogicTests
    {
        int offset = (int)Math.Round(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes) * -1;

        [TestMethod]
        public async Task Load_DeviceTelemetryReportDataLastThreeHoursAsync()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                DateTime timestamp = DateTime.UtcNow;
                DeviceTelemetryReportModel result = await telemetryLogic.LoadDeviceTelemetryReportDataLastThreeHoursAsync(TestConfigHelper.DeviceId, timestamp, offset);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public async Task Load_DeviceSummaryDataPerHour()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                DateTime timestamp = DateTime.UtcNow;
                DeviceTelemetryReportModel result = await telemetryLogic.LoadDeviceTelemetryReportDataPerHourAsync(TestConfigHelper.DeviceId, timestamp, offset);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public async Task Load_DeviceSummaryDataPerDay()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                DateTime test = DateTime.UtcNow.AddDays(-1);
                DeviceTelemetryReportModel result = await telemetryLogic.LoadDeviceTelemetryReportDataPerDayAsync(TestConfigHelper.DeviceId, test, offset);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public async Task Load_DeviceSummaryDataPerWeek()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                DeviceTelemetryReportModel result = await telemetryLogic.LoadDeviceTelemetryReportDataPerWeekAsync(TestConfigHelper.DeviceId, DateTime.UtcNow, offset);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public async Task Load_DeviceSummaryDataPerMonth()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                DeviceTelemetryReportModel result = await telemetryLogic.LoadDeviceTelemetryReportDataPerMonthAsync(TestConfigHelper.DeviceId, DateTime.UtcNow, offset);
                Assert.IsNotNull(result);
            }
        }
    }
}
