using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public interface IDeviceTelemetryLogic
    {
        Task<DeviceTelemetryModel> LoadLatestDeviceTelemetryAsync(
            string deviceId);

        Task<IEnumerable<DeviceTelemetryModel>> LoadLatestDeviceTelemetryAsync(
            string deviceId,
            DateTime minTime,
            int offset);

        Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataLastThreeHoursAsync(
            string deviceId,
            DateTime loadUntil,
            int timezoneOffset);

        Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime? minTime,
            int offset);

        Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataPerHourAsync(
            string deviceId,
            DateTime minTime,
            int offset);

        Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataPerDayAsync(
           string deviceId,
           DateTime minTime,
           int offset);

        Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataPerWeekAsync(
           string deviceId,
           DateTime minTime,
           int offset);

        Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataPerMonthAsync(
           string deviceId,
           DateTime minTime,
           int offset);


        //        Func<string, DateTime?> ProduceGetLatestDeviceAlertTime(
        //            IEnumerable<AlertHistoryItemModel> alertHistoryModels);
    }
}
