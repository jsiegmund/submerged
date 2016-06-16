using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public interface IDeviceTelemetryRepository
    {
        Task<DeviceTelemetryModel> LoadLatestDeviceTelemetryAsync(
            string deviceId);

        Task<IEnumerable<DeviceTelemetryModel>> LoadLatestDeviceTelemetryAsync(
            string deviceId,
            DateTimeOffset minTimeUTC);


        Task<IEnumerable<DeviceTelemetryModel>> LoadDeviceTelemetryAsync(
            string deviceId,
            DateTimeOffset minTimeUTC,
            DateTimeOffset maxTimeUTC);

        Task<DeviceTelemetrySummaryModel> LoadDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTimeOffset minTimeUTC);

        Task<IEnumerable<DeviceTelemetrySummaryModel>> LoadDeviceTelemetrySummaryAsync(
             string deviceId,
            DateTimeOffset minTimeUTC,
            DateTimeOffset maxTimeUTC);
    }
}
