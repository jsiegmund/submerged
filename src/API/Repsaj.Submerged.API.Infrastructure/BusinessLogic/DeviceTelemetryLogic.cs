using Repsaj.Submerged.Common.Helpers;
using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Infrastructure.Models;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public class DeviceTelemetryLogic : IDeviceTelemetryLogic
    {
        private readonly IDeviceTelemetryRepository _deviceTelemetryRepository;

        /// <summary>
        /// Initializes a new instance of the DeviceTelemetryLogic class.
        /// </summary>
        /// <param name="deviceTelemetryRepository">
        /// The IDeviceTelemetryRepository implementation that the new 
        /// instance will use.
        /// </param>
        public DeviceTelemetryLogic(IDeviceTelemetryRepository deviceTelemetryRepository)
        {
            if (deviceTelemetryRepository == null)
            {
                throw new ArgumentNullException("deviceTelemetryRepository");
            }

            _deviceTelemetryRepository = deviceTelemetryRepository;
        }

        public async Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataPerHourAsync(string deviceId, DateTimeOffset loadUntilUTC, int timeOffsetSeconds)
        {
            DeviceTelemetryReportModel model = new DeviceTelemetryReportModel();
            model.SerieLabels = new string[] { "Temperature 1", "Temperature 2", "pH" };

            DateTimeOffset startUTC = loadUntilUTC.AddHours(-1);
            DateTimeOffset endUTC = loadUntilUTC;

            List<string> labels = new List<string>();

            for (int i = startUTC.Minute; labels.Count() < 60; i++)
            {
                int hour = i % 60;
                labels.Add(hour.ToString().PadLeft(2, '0'));
            }

            model.DataLabels = labels.ToArray();

            // fetch the data from the repository, startin on minTime and adding one day
            IEnumerable<DeviceTelemetryModel> telemetryData = await _deviceTelemetryRepository.LoadDeviceTelemetryAsync(deviceId, startUTC, endUTC);

            // validate the returned items are correct 
            if (telemetryData.Max(t => t.EventEnqueuedUTCTime) > endUTC || 
                telemetryData.Min(t => t.EventEnqueuedUTCTime) < startUTC)
                throw new ArgumentException("Returned data range is invalid");

            var data = from t in telemetryData
                       orderby t.EventEnqueuedUTCTime.Value
                       group t by t.EventEnqueuedUTCTime.Value.AddSeconds(timeOffsetSeconds).Minute into g
                       select new GroupedTelemetryModel
                       {
                           Key = g.Key,
                           Temperature1 = g.Average(d => d.Temperature1),
                           Temperature2 = g.Average(d => d.Temperature2),
                           pH = g.Average(d => d.pH)
                       };
            
            var dataSeries = MiscHelper.PadTelemetryData(data, startUTC.Minute, 60, 60);
            model.DataSeries = dataSeries;

            return model;
        }

        public async Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataLastThreeHoursAsync(string deviceId, DateTimeOffset dateUTC, int timeOffsetSeconds)
        {
            DateTimeOffset localDate = dateUTC.AddSeconds(timeOffsetSeconds);

            DeviceTelemetryReportModel model = new DeviceTelemetryReportModel();
            model.SerieLabels = new string[] { "Temperature 1", "Temperature 2", "pH" };
            model.DataLabels = new string[] {
                localDate.AddHours(-3).ToString("HH:mm"),
                localDate.AddHours(-2.5).ToString("HH:mm"),
                localDate.AddHours(-2).ToString("HH:mm"),
                localDate.AddHours(-1.5).ToString("HH:mm"),
                localDate.AddHours(-1).ToString("HH:mm"),
                localDate.AddHours(-0.5).ToString("HH:mm"),
            };

            DateTimeOffset windowUTCStart = dateUTC.AddHours(-3);
            DateTimeOffset windowUTCEnd = dateUTC;

            // fetch the data from the repository, startin on minTime and adding one day
            IEnumerable<DeviceTelemetryModel> telemetryData = await _deviceTelemetryRepository.LoadDeviceTelemetryAsync(deviceId, windowUTCStart, windowUTCEnd);

            // validate the returned items are correct 
            if (telemetryData.Max(t => t.EventEnqueuedUTCTime) > windowUTCEnd ||
                telemetryData.Min(t => t.EventEnqueuedUTCTime) < windowUTCStart)
                throw new ArgumentException("Returned data range is invalid");

            // project the data to find half-hour segments 
            var projectedData = telemetryData.Select(d => new {
                pH = d.pH,
                Temperature1 = d.Temperature1,
                Temperature2 = d.Temperature2,
                Segment = MiscHelper.ProjectHalfHourSegments(windowUTCStart.DateTime, d.EventEnqueuedUTCTime.Value)
            });

            var data = from t in projectedData
                       orderby t.Segment
                       group t by t.Segment into g
                       select new GroupedTelemetryModel
                       {
                           Key = g.Key,
                           Temperature1 = g.Average(d => d.Temperature1),
                           Temperature2 = g.Average(d => d.Temperature2),
                           pH = g.Average(d => d.pH)
                       };

            var dataSeries = MiscHelper.PadTelemetryData(data, 0, 6);
            model.DataSeries = dataSeries;

            return model;
        }

        public async Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataPerDayAsync(string deviceId, DateTimeOffset dateUTC, int timeOffsetSeconds)
        {
            DeviceTelemetryReportModel model = new DeviceTelemetryReportModel();
            model.SerieLabels = new string[] { "Temperature 1", "Temperature 2", "pH" };
            model.DataLabels = new string[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23" };

            // calculate the day in the users timezone 
            DateTime localDate = dateUTC.AddSeconds(timeOffsetSeconds).Date;
            DateTime windowUTCStart = localDate.AddSeconds(timeOffsetSeconds);
            DateTime windowUTCEnd = windowUTCStart.AddDays(1);

            // fetch the data from the repository, startin on minTime and adding one day
            IEnumerable<DeviceTelemetrySummaryModel> telemetryData = await _deviceTelemetryRepository.LoadDeviceTelemetrySummaryAsync(deviceId, windowUTCStart, windowUTCEnd);

            var data = from t in telemetryData
                       group t by t.OutTime.Value.AddSeconds(timeOffsetSeconds).Hour into g
                       select new GroupedTelemetryModel()
                       {
                           Key = g.Key,
                           Temperature1 = g.Average(d => d.AverageTemp1),
                           Temperature2 = g.Average(d => d.AverageTemp2),
                           pH = g.Average(d => d.AveragePH)
                       };

            var dataSeries = MiscHelper.PadTelemetryData(data, 0, 24);
            model.DataSeries = dataSeries;

            return model;
        }
    
        public async Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataPerWeekAsync(string deviceId, DateTimeOffset minTimeUTC, int timeOffsetSeconds)
        {
            DeviceTelemetryReportModel model = new DeviceTelemetryReportModel();
            model.SerieLabels = new string[] { "Temperature 1", "Temperature 2", "pH" };
            model.DataLabels = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            // calculate the monday of this week, timestamp 00:00
            DateTime monday = minTimeUTC.Date;
            while (monday.DayOfWeek != DayOfWeek.Monday)
                monday = monday.AddDays(-1);

            // add 7 days, which gives us the next monday 00:00
            DateTime sunday = monday.AddDays(7).AddSeconds(-1);

            // fetch the data from the repository, startin on minTime and adding one day
            IEnumerable<DeviceTelemetrySummaryModel> telemetryData = await _deviceTelemetryRepository.LoadDeviceTelemetrySummaryAsync(deviceId, monday, sunday);

            var data = from t in telemetryData
                       group t by t.Timestamp.Value.AddSeconds(timeOffsetSeconds).Day into g
                       select new GroupedTelemetryModel()
                       {
                           Key = g.Key,
                           Temperature1 = g.Average(d => d.AverageTemp1),
                           Temperature2 = g.Average(d => d.AverageTemp2),
                           pH = g.Average(d => d.AveragePH)
                       };

            var dataSeries = MiscHelper.PadTelemetryData(data, monday.Day, 7);
            model.DataSeries = dataSeries;

            return model;

        }

        public async Task<DeviceTelemetryReportModel> LoadDeviceTelemetryReportDataPerMonthAsync(string deviceId, DateTimeOffset dateUTC, int timeOffsetSeconds)
        {
            DeviceTelemetryReportModel model = new DeviceTelemetryReportModel();
            model.SerieLabels = new string[] { "Temperature 1", "Temperature 2", "pH" };

            List<string> label = new List<string>();

            int daysInMonth = DateTime.DaysInMonth(dateUTC.Year, dateUTC.Month);
            for (int i = 0; i < daysInMonth; i++)
            {
                label.Add(i.ToString().PadLeft(2, '0'));
            }

            model.DataLabels = label.ToArray();

            DateTime start = new DateTime(dateUTC.Year, dateUTC.Month, 1);
            DateTime end = new DateTime(dateUTC.Year, dateUTC.Month, daysInMonth);

            // fetch the data from the repository, startin on minTime and adding one day
            IEnumerable<DeviceTelemetrySummaryModel> telemetryData = await _deviceTelemetryRepository.LoadDeviceTelemetrySummaryAsync(deviceId, start, end);

            var data = from t in telemetryData
                       group t by t.Timestamp.Value.AddSeconds(timeOffsetSeconds).Day into g
                       select new GroupedTelemetryModel()
                       {
                           Key = g.Key,
                           Temperature1 = g.Average(d => d.AverageTemp1),
                           Temperature2 = g.Average(d => d.AverageTemp2),
                           pH = g.Average(d => d.AveragePH)
                       };

            var dataSeries = MiscHelper.PadTelemetryData(data, 1, daysInMonth);
            model.DataSeries = dataSeries;

            return model;
        }

        /// <summary>
        /// Loads the most recent Device telemetry.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which telemetry should be returned.
        /// </param>
        /// <returns>
        /// The last registered telemetry for the Device specified by deviceId
        /// </returns>
        public async Task<DeviceTelemetryModel> LoadLatestDeviceTelemetryAsync(
            string deviceId)
        {
            return await _deviceTelemetryRepository.LoadLatestDeviceTelemetryAsync(deviceId);
        }


        /// <summary>
        /// Loads the most recent Device telemetry.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which telemetry should be returned.
        /// </param>
        /// <param name="minTimeUTC">
        /// The minimum time of record of the telemetry that should be returned.
        /// </param>
        /// <returns>
        /// Telemetry for the Device specified by deviceId, inclusively since 
        /// minTime.
        /// </returns>
        public async Task<IEnumerable<DeviceTelemetryModel>> LoadDeviceTelemetryAsync(
            string deviceId,
            DateTimeOffset minTimeUTC, 
            int timeOffsetSeconds)
        {
            return await _deviceTelemetryRepository.LoadLatestDeviceTelemetryAsync(deviceId, minTimeUTC);
        }

        /// <summary>
        /// Loads the most recent DeviceTelemetrySummaryModel for a specified Device.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which a telemetry summary model should be 
        /// returned.
        /// </param>
        /// <param name="minTimeUTC">
        /// If provided the the minimum time stamp of the summary data that should 
        /// be loaded. 
        /// </param>
        /// <returns>
        /// The most recent DeviceTElemetrySummaryModel for the Device, 
        /// specified by deviceId.
        /// </returns>
        public async Task<DeviceTelemetrySummaryModel> LoadDeviceTelemetrySummaryAsync(string deviceId, DateTimeOffset minTimeUTC, int timeOffsetSeconds)
        {
            return await _deviceTelemetryRepository.LoadDeviceTelemetrySummaryAsync(deviceId, minTimeUTC);
        }

        /// <summary>
        /// Produces a delegate for getting the time of a specified Device's most
        /// recent alert.
        /// </summary>
        /// <param name="alertHistoryModels">
        /// A collection of AlertHistoryItemModel, representing all alerts that 
        /// should be considered.
        /// </param>
        /// <returns>
        /// A delegate for getting the time of a specified Device's most recent 
        /// alert.
        /// </returns>
        //public Func<string, DateTime?> ProduceGetLatestDeviceAlertTime(
        //    IEnumerable<AlertHistoryItemModel> alertHistoryModels)
        //{
        //    DateTime date;

        //    if (alertHistoryModels == null)
        //    {
        //        throw new ArgumentNullException("alertHistoryModels");
        //    }

        //    Dictionary<string, DateTime> index = new Dictionary<string, DateTime>();

        //    alertHistoryModels = alertHistoryModels.Where(
        //        t =>
        //            (t != null) &&
        //            !string.IsNullOrEmpty(t.DeviceId) &&
        //            t.Timestamp.HasValue);

        //    foreach (AlertHistoryItemModel model in alertHistoryModels)
        //    {
        //        if (index.TryGetValue(model.DeviceId, out date) && (date >= model.Timestamp))
        //        {
        //            continue;
        //        }

        //        index[model.DeviceId] = model.Timestamp.Value;
        //    }

        //    return (deviceId) =>
        //    {
        //        DateTime lastAlert;

        //        if (index.TryGetValue(deviceId, out lastAlert))
        //        {
        //            return lastAlert;
        //        }

        //        return null;
        //    };
        //}
    }
}
