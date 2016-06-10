using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.Helpers;
using Repsaj.Submerged.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    using Microsoft.WindowsAzure.Storage;
    using System.Globalization;
    using System.Net;
    using StrDict = IDictionary<string, string>;

    public class DeviceTelemetryRepository : IDeviceTelemetryRepository
    {
        private readonly string _telemetryContainerName;
        private readonly string _telemetryDataPrefix;
        private readonly string _telemetryStoreConnectionString;
        private readonly string _telemetrySummaryPrefix;

        /// <summary>
        /// Initializes a new instance of the DeviceTelemetryRepository class.
        /// </summary>
        /// <param name="configProvider">
        /// The IConfigurationProvider implementation with which to initialize 
        /// the new instance.
        /// </param>
        public DeviceTelemetryRepository(IConfigurationProvider configProvider)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            _telemetryContainerName = configProvider.GetConfigurationSettingValue("TelemetryStoreContainerName");
            _telemetryDataPrefix = configProvider.GetConfigurationSettingValue("TelemetryDataPrefix");
            _telemetryStoreConnectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _telemetrySummaryPrefix = configProvider.GetConfigurationSettingValue("TelemetrySummaryPrefix");
        }

        /// <summary>
        /// Loads the most recent Device telemetry.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which telemetry should be returned.
        /// </param>
        /// <param name="minTime">
        /// The minimum time of record of the telemetry that should be returned.
        /// </param>
        /// <returns>
        /// Telemetry for the Device specified by deviceId, inclusively since 
        /// minTime.
        /// </returns>
        public async Task<DeviceTelemetryModel> LoadLatestDeviceTelemetryAsync(
            string deviceId)
        {
            DeviceTelemetryModel result = null;

            CloudBlobContainer container =
                await BlobStorageHelper.BuildBlobContainerAsync(this._telemetryStoreConnectionString, _telemetryContainerName);

            IEnumerable<IListBlobItem> blobs =
                await BlobStorageHelper.LoadBlobItemsAsync(
                    async (token) =>
                    {
                        return await container.ListBlobsSegmentedAsync(
                            _telemetryDataPrefix,
                            true,
                            BlobListingDetails.None,
                            null,
                            token,
                            null,
                            null);
                    });

            blobs = blobs.OrderByDescending(t => BlobStorageHelper.ExtractBlobItemDate(t));

            CloudBlockBlob blockBlob;
            IEnumerable<DeviceTelemetryModel> blobModels;
            foreach (IListBlobItem blob in blobs)
            {
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }

                try
                {
                    blobModels = await LoadBlobTelemetryModelsAsync(blockBlob);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                int preFilterCount = blobModels.Count();

                blobModels =
                    blobModels.Where(
                        t => (t != null) &&
                             t.Timestamp.HasValue)
                              .OrderByDescending(
                        t => t.Timestamp.Value);

                if (preFilterCount == 0)
                {
                    break;
                }

                if (!string.IsNullOrEmpty(deviceId))
                {
                    blobModels = blobModels.Where(t => t.DeviceId == deviceId);
                }

                result = blobModels.FirstOrDefault();
                if (result != null)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Loads the most recent Device telemetry.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which telemetry should be returned.
        /// </param>
        /// <param name="minTime">
        /// The minimum time of record of the telemetry that should be returned.
        /// </param>
        /// <returns>
        /// Telemetry for the Device specified by deviceId, inclusively since 
        /// minTime.
        /// </returns>
        public async Task<IEnumerable<DeviceTelemetryModel>> LoadLatestDeviceTelemetryAsync(
            string deviceId,
            DateTime minTime)
        {
            IEnumerable<DeviceTelemetryModel> result = new DeviceTelemetryModel[0];

            CloudBlobContainer container =
                await BlobStorageHelper.BuildBlobContainerAsync(this._telemetryStoreConnectionString, _telemetryContainerName);

            IEnumerable<IListBlobItem> blobs =
                await BlobStorageHelper.LoadBlobItemsAsync(
                    async (token) =>
                    {
                        return await container.ListBlobsSegmentedAsync(
                            _telemetryDataPrefix,
                            true,
                            BlobListingDetails.None,
                            null,
                            token,
                            null,
                            null);
                    });

            blobs = blobs.OrderByDescending(t => BlobStorageHelper.ExtractBlobItemDate(t));

            CloudBlockBlob blockBlob;
            IEnumerable<DeviceTelemetryModel> blobModels;
            foreach (IListBlobItem blob in blobs)
            {
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }

                try
                {
                    blobModels = await LoadBlobTelemetryModelsAsync(blockBlob);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                int preFilterCount = blobModels.Count();

                blobModels =
                    blobModels.Where(
                        t =>
                            (t != null) &&
                            t.Timestamp.HasValue &&
                            t.Timestamp.Value >= minTime);

                if (preFilterCount == 0)
                {
                    break;
                }

                result = result.Concat(blobModels);

                if (preFilterCount != blobModels.Count())
                {
                    break;
                }
            }

            if (!string.IsNullOrEmpty(deviceId))
            {
                result = result.Where(t => t.DeviceId == deviceId);
            }

            return result;
        }

        /// <summary>
        /// Loads the most recent Device telemetry.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which telemetry should be returned.
        /// </param>
        /// <param name="minTime">
        /// The minimum time of record of the telemetry that should be returned.
        /// </param>
        /// <returns>
        /// Telemetry for the Device specified by deviceId, inclusively since 
        /// minTime.
        /// </returns>
        public async Task<IEnumerable<DeviceTelemetryModel>> LoadDeviceTelemetryAsync(
            string deviceId,
            DateTime minTime, 
            DateTime maxTime)
        {
            IEnumerable<DeviceTelemetryModel> result = new DeviceTelemetryModel[0];

            CloudBlobContainer container =
                await BlobStorageHelper.BuildBlobContainerAsync(this._telemetryStoreConnectionString, _telemetryContainerName);

            IEnumerable<IListBlobItem> blobs =
                await BlobStorageHelper.LoadBlobItemsAsync(
                    async (token) =>
                    {
                        return await container.ListBlobsSegmentedAsync(
                            _telemetryDataPrefix,
                            true,
                            BlobListingDetails.None,
                            null,
                            token,
                            null,
                            null);
                    });

            // remove all the blobs that were made before the minTime we're looking for, they 
            // wont contain relevant data 
            blobs = blobs.OrderByDescending(t => BlobStorageHelper.ExtractBlobItemDate(t));

            CloudBlockBlob blockBlob;
            IEnumerable<DeviceTelemetryModel> blobModels;
            foreach (IListBlobItem blob in blobs)
            {
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }

                // Translate LastModified to local time zone.  DateTimeOffsets 
                // don't do this automatically.  This is for equivalent behavior 
                // with parsed DateTimes.
                if ((blockBlob.Properties != null) &&
                    blockBlob.Properties.LastModified.HasValue &&
                    (blockBlob.Properties.LastModified.Value < minTime))
                {
                    break;
                }

                try
                {
                    blobModels = await LoadBlobTelemetryModelsAsync(blockBlob);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                int preFilterCount = blobModels.Count();

                blobModels =
                    blobModels.Where(
                        t =>
                            (t != null) &&
                            t.Timestamp.HasValue &&
                            t.Timestamp.Value >= minTime && 
                            t.Timestamp.Value <= maxTime);

                if (preFilterCount == 0)
                {
                    break;
                }

                result = result.Concat(blobModels);

                if (preFilterCount != blobModels.Count())
                {
                    break;
                }
            }

            if (!string.IsNullOrEmpty(deviceId))
            {
                result = result.Where(t => t.DeviceId == deviceId);
            }

            return result;
        }

        private async static Task<List<DeviceTelemetryModel>> LoadBlobTelemetryModelsAsync(CloudBlockBlob blob)
        {
            Debug.Assert(blob != null, "blob is a null reference.");

            List<DeviceTelemetryModel> models = new List<DeviceTelemetryModel>();

            TextReader reader = null;
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream();
                OperationContext context = new OperationContext();
                BlobRequestOptions options = new BlobRequestOptions()
                {
                     
                };
                await blob.DownloadToStreamAsync(stream, null, options, context);
                stream.Position = 0;
                reader = new StreamReader(stream);

                IEnumerable<StrDict> strdicts = ParsingHelper.ParseCsv(reader).ToDictionaries();
                DeviceTelemetryModel model;
                string str;
                double number;
                bool boolean;
                foreach (StrDict strdict in strdicts)
                {
                    model = new DeviceTelemetryModel();

                    if (strdict.TryGetValue("deviceid", out str))
                    {
                        model.DeviceId = str;
                    }

                    if (strdict.TryGetValue("temperature1", out str) &&
                        double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.Temperature1 = number;
                    }

                    if (strdict.TryGetValue("temperature2", out str) &&
                        double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.Temperature2 = number;
                    }

                    if (strdict.TryGetValue("ph", out str) &&
                        double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.pH = number;
                    }

                    if (strdict.TryGetValue("leakdetected", out str) &&
                        Boolean.TryParse(
                            str,
                            out boolean))
                    {
                        model.LeakDetected = boolean;
                    }

                    if (strdict.TryGetValue("leaksensors", out str))
                    {
                        model.LeakSensors = str;
                    }

                    DateTime date;
                    if (strdict.TryGetValue("eventenqueuedutctime", out str) &&
                        DateTime.TryParse(
                            str,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AllowWhiteSpaces,
                            out date))
                    {
                        model.Timestamp = date;
                    }

                    models.Add(model);
                }
            }
            finally
            {
                IDisposable disp;

                if ((disp = stream) != null)
                {
                    disp.Dispose();
                }

                if ((disp = reader) != null)
                {
                    disp.Dispose();
                }
            }

            return models;
        }

        /// <summary>
        /// Loads the most recent DeviceTelemetrySummaryModel for a specified Device.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which a telemetry summary model should be 
        /// returned.
        /// </param>
        /// <param name="minTime">
        /// If provided the the minimum time stamp of the summary data that should 
        /// be loaded.
        /// </param>
        /// <returns>
        /// The most recent DeviceTelemetrySummaryModel for the Device, 
        /// specified by deviceId.
        /// </returns>
        public async Task<DeviceTelemetrySummaryModel> LoadLatestDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime? minTime)
        {
            DeviceTelemetrySummaryModel summaryModel = null;

            CloudBlobContainer container =
                await BlobStorageHelper.BuildBlobContainerAsync(
                    this._telemetryStoreConnectionString,
                    _telemetryContainerName);

            IEnumerable<IListBlobItem> blobs =
                await BlobStorageHelper.LoadBlobItemsAsync(
                    async (token) =>
                    {
                        return await container.ListBlobsSegmentedAsync(
                            _telemetrySummaryPrefix,
                            true,
                            BlobListingDetails.None,
                            null,
                            token,
                            null,
                            null);
                    });

            blobs = blobs.OrderByDescending(t => BlobStorageHelper.ExtractBlobItemDate(t));

            IEnumerable<DeviceTelemetrySummaryModel> blobModels;
            CloudBlockBlob blockBlob;

            foreach (IListBlobItem blob in blobs)
            {
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }

                // Translate LastModified to local time zone.  DateTimeOffsets 
                // don't do this automatically.  This is for equivalent behavior 
                // with parsed DateTimes.
                if (minTime.HasValue &&
                    (blockBlob.Properties != null) &&
                    blockBlob.Properties.LastModified.HasValue &&
                    (blockBlob.Properties.LastModified.Value < minTime.Value))
                {
                    break;
                }

                try
                {
                    blobModels = await LoadBlobTelemetrySummaryModelsAsync(blockBlob);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                blobModels = blobModels.Where(t => t != null);

                if (!string.IsNullOrEmpty(deviceId))
                {
                    blobModels = blobModels.Where(t => t.DeviceId == deviceId);
                }

                summaryModel = blobModels.LastOrDefault();
                if (summaryModel != null)
                {
                    break;
                }
            }

            return summaryModel;
        }

        /// <summary>
        /// Loads the most recent DeviceTelemetrySummaryModel for a specified Device.
        /// </summary>
        /// <param name="deviceId">
        /// The ID of the Device for which a telemetry summary model should be 
        /// returned.
        /// </param>
        /// <param name="minTime">
        /// If provided the the minimum time stamp of the summary data that should 
        /// be loaded.
        /// </param>
        /// <returns>
        /// The most recent DeviceTelemetrySummaryModel for the Device, 
        /// specified by deviceId.
        /// </returns>
        public async Task<IEnumerable<DeviceTelemetrySummaryModel>> LoadDeviceTelemetrySummaryAsync(
            string deviceId,
            DateTime minTime,
            DateTime maxTime)
        {
            CloudBlobContainer container =
                await BlobStorageHelper.BuildBlobContainerAsync(
                    this._telemetryStoreConnectionString,
                    _telemetryContainerName);

            IEnumerable<IListBlobItem> blobs =
                await BlobStorageHelper.LoadBlobItemsAsync(
                    async (token) =>
                    {
                        return await container.ListBlobsSegmentedAsync(
                            _telemetrySummaryPrefix,
                            true,
                            BlobListingDetails.None,
                            null,
                            token,
                            null,
                            null);
                    });

            blobs = blobs.OrderByDescending(t => BlobStorageHelper.ExtractBlobItemDate(t));

            IEnumerable<DeviceTelemetrySummaryModel> blobModels;
            List<DeviceTelemetrySummaryModel> result = new List<DeviceTelemetrySummaryModel>();
            CloudBlockBlob blockBlob;

            foreach (IListBlobItem blob in blobs)
            {
                if ((blockBlob = blob as CloudBlockBlob) == null)
                {
                    continue;
                }

                // Translate LastModified to local time zone.  DateTimeOffsets 
                // don't do this automatically.  This is for equivalent behavior 
                // with parsed DateTimes.
                if ((blockBlob.Properties != null) &&
                    blockBlob.Properties.LastModified.HasValue &&
                    (blockBlob.Properties.LastModified.Value < minTime ||
                     blockBlob.Properties.LastModified.Value > maxTime))
                {
                    // when the datetime range is outside of the scope; continue to the next blob
                    continue;
                }

                try
                {
                    blobModels = await LoadBlobTelemetrySummaryModelsAsync(blockBlob);
                }
                catch
                {
                    continue;
                }

                if (blobModels == null)
                {
                    break;
                }

                // filter the models based on the given datetime range
                blobModels = blobModels.Where(t => t != null &&
                                                   t.OutTime != null &&
                                                   t.OutTime >= minTime && t.OutTime <= maxTime);

                if (!string.IsNullOrEmpty(deviceId))
                {
                    blobModels = blobModels.Where(t => t.DeviceId == deviceId);
                }

                result.AddRange(blobModels);
            }

            result = result.OrderBy(r => r.OutTime).ToList();

            return result;
        }

        private async static Task<List<DeviceTelemetrySummaryModel>> LoadBlobTelemetrySummaryModelsAsync(
            CloudBlockBlob blob)
        {
            Debug.Assert(blob != null, "blob is a null reference.");

            var models = new List<DeviceTelemetrySummaryModel>();

            TextReader reader = null;
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);
                stream.Position = 0;
                reader = new StreamReader(stream);

                IEnumerable<StrDict> strdicts = ParsingHelper.ParseCsv(reader).ToDictionaries();
                DeviceTelemetrySummaryModel model;
                DateTime datetime;
                double number;
                string str;
                foreach (StrDict strdict in strdicts)
                {
                    model = new DeviceTelemetrySummaryModel();

                    if (strdict.TryGetValue("deviceid", out str))
                    {
                        model.DeviceId = str;
                    }

                    if (strdict.TryGetValue("averagetemperature1", out str) &&
                       double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.AverageTemp1 = number;
                    }

                    if (strdict.TryGetValue("averagetemperature2", out str) &&
                       double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.AverageTemp2 = number;
                    }

                    if (strdict.TryGetValue("averageph", out str) &&
                       double.TryParse(
                            str,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out number))
                    {
                        model.AveragePh = number;
                    }

                    if (strdict.TryGetValue("outtime", out str) &&
                       DateTime.TryParse(
                            str,
                            out datetime))
                    {
                        model.OutTime = datetime;
                    }

                    // Translate LastModified to local time zone.  DateTimeOffsets 
                    // don't do this automatically.  This is for equivalent behavior 
                    // with parsed DateTimes.
                    if ((blob.Properties != null) &&
                        blob.Properties.LastModified.HasValue)
                    {
                        model.Timestamp = blob.Properties.LastModified.Value.DateTime;
                    }

                    models.Add(model);
                }
            }
            finally
            {
                IDisposable disp;
                if ((disp = stream) != null)
                {
                    disp.Dispose();
                }

                if ((disp = reader) != null)
                {
                    disp.Dispose();
                }
            }

            return models;
        }
    }
}
