using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repsaj.Submerged.Infrastructure.Repository;
using Repsaj.Submerged.Infrastructure.Models;
using System.Threading.Tasks;

namespace Repsaj.Submerged.APITests
{
    [TestClass]
    public class DeviceTelemetryRepositoryTests : TestBase
    {
        int offset = (int)Math.Round(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes) * -1;

        [TestMethod]
        public void CanLoadLatestDeviceTelemetryAsync()
        {
            Task<DeviceTelemetryModel> task = DeviceTelemetryRepository.LoadLatestDeviceTelemetryAsync("");
            task.Wait();
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        public void CanLoadDeviceTelemetryReportDataLastThreeHoursAsync()
        {
            DateTime timestamp = DateTime.UtcNow;
            Task<DeviceTelemetryReportModel> task = DeviceTelemetryLogic.LoadDeviceTelemetryReportDataLastThreeHoursAsync("", timestamp, offset);
            task.Wait();
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        public void CanLoadDeviceSummaryDataPerHour()
        {
            DateTime timestamp = DateTime.UtcNow;
            Task<DeviceTelemetryReportModel> task = DeviceTelemetryLogic.LoadDeviceTelemetryReportDataPerHourAsync("", timestamp, offset);
            task.Wait();
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        public void CanLoadDeviceSummaryDataPerDay()
        {
            DateTime test = DateTime.UtcNow;
            Task<DeviceTelemetryReportModel> task = DeviceTelemetryLogic.LoadDeviceTelemetryReportDataPerDayAsync("", test, offset);
            task.Wait();
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        public void CanLoadDeviceSummaryDataPerWeek()
        {
            Task<DeviceTelemetryReportModel> task = DeviceTelemetryLogic.LoadDeviceTelemetryReportDataPerWeekAsync("", DateTime.UtcNow, offset);
            task.Wait();
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        public void CanLoadDeviceSummaryDataPerMonth()
        {
            Task<DeviceTelemetryReportModel> task = DeviceTelemetryLogic.LoadDeviceTelemetryReportDataPerMonthAsync("", DateTime.UtcNow, offset);
            task.Wait();
            Assert.IsNotNull(task.Result);
        }
    }
}
