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
            DateTime minTime);


        Task<IEnumerable<DeviceTelemetryModel>> LoadDeviceTelemetryAsync(
            string deviceId,
            DateTime minTime,
            DateTime maxTime);

        Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime? minTime);

        Task<IEnumerable<DeviceTelemetrySummaryModel>> LoadDeviceTelemetrySummaryAsync(
             string deviceId,
            DateTime minTime,
            DateTime maxTime);
    }
}
