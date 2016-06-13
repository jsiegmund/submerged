﻿using System;
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
        public void CanLoadDeviceTelemetryReportDataLastThreeHoursAsync()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                DateTime timestamp = DateTime.UtcNow;
                Task<DeviceTelemetryReportModel> task = telemetryLogic.LoadDeviceTelemetryReportDataLastThreeHoursAsync(TestConfigHelper.DeviceId, timestamp, offset);
                task.Wait();
                Assert.IsNotNull(task.Result);
            }
        }

        [TestMethod]
        public void CanLoadDeviceSummaryDataPerHour()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                DateTime timestamp = DateTime.UtcNow;
                Task<DeviceTelemetryReportModel> task = telemetryLogic.LoadDeviceTelemetryReportDataPerHourAsync(TestConfigHelper.DeviceId, timestamp, offset);
                task.Wait();
                Assert.IsNotNull(task.Result);
            }
        }

        [TestMethod]
        public void CanLoadDeviceSummaryDataPerDay()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                DateTime test = DateTime.UtcNow;
                Task<DeviceTelemetryReportModel> task = telemetryLogic.LoadDeviceTelemetryReportDataPerDayAsync(TestConfigHelper.DeviceId, test, offset);
                task.Wait();
                Assert.IsNotNull(task.Result);
            }
        }

        [TestMethod]
        public void CanLoadDeviceSummaryDataPerWeek()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                Task<DeviceTelemetryReportModel> task = telemetryLogic.LoadDeviceTelemetryReportDataPerWeekAsync(TestConfigHelper.DeviceId, DateTime.UtcNow, offset);
                task.Wait();
                Assert.IsNotNull(task.Result);
            }
        }

        [TestMethod]
        public void CanLoadDeviceSummaryDataPerMonth()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var telemetryLogic = autoMock.Create<DeviceTelemetryLogic>();
                Task<DeviceTelemetryReportModel> task = telemetryLogic.LoadDeviceTelemetryReportDataPerMonthAsync(TestConfigHelper.DeviceId, DateTime.UtcNow, offset);
                task.Wait();
                Assert.IsNotNull(task.Result);
            }
        }
    }
}
