using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.DeviceSchema;
using Repsaj.Submerged.Common.Utility;
using Repsaj.Submerged.Infrastructure.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public class DeviceRegistryRepository : IDeviceRegistryCrudRepository
    {
        // Configuration strings for use in accessing the DocumentDB, Database and DocumentCollection
        readonly string _endpointUri;
        readonly string _authorizationKey;
        readonly string _databaseId;
        readonly string _documentCollectionName;

        IDocDbRestUtility _docDbRestUtil;
        IDocDbOperations _docDbOperations;

        public DeviceRegistryRepository(IConfigurationProvider configProvider, IDocDbRestUtility docDbRestUtil, IDocDbOperations docDbOperations)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            _endpointUri = configProvider.GetConfigurationSettingValue("docdb.EndpointUrl");
            _authorizationKey = configProvider.GetConfigurationSettingValue("docdb.PrimaryAuthorizationKey");
            _databaseId = configProvider.GetConfigurationSettingValue("docdb.DatabaseId");
            _documentCollectionName = configProvider.GetConfigurationSettingValue("docdb.DocumentCollectionId");


            _docDbOperations = docDbOperations;
            _docDbRestUtil = docDbRestUtil;
            Task.Run(() => _docDbRestUtil.InitializeDatabase()).Wait();
            Task.Run(() => _docDbRestUtil.InitializeCollection()).Wait();
        }

        /// <summary>
        /// Adds a device to the DocumentDB.
        /// Throws a DeviceAlreadyRegisteredException if a device already exists in the database with the provided deviceId
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<dynamic> AddDeviceAsync(dynamic device)
        {
            string deviceId = DeviceSchemaHelper.GetDeviceID(device);
            dynamic existingDevice = await GetDeviceAsync(deviceId);

            if (existingDevice != null)
            {
                throw new DeviceAlreadyRegisteredException(deviceId);
            }

            device = await _docDbOperations.SaveNewDocumentAsync(device);

            return device;
        }

        /// <summary>
        /// Queries the DocumentDB and retrieves the device based on its deviceId
        /// </summary>
        /// <param name="deviceId">DeviceID of the device to retrieve</param>
        /// <returns>Device instance if present, null if a device was not found with the provided deviceId</returns>
        public async Task<dynamic> GetDeviceAsync(string deviceId)
        {
            dynamic result = null;

            Dictionary<string, Object> queryParams = new Dictionary<string, Object>();
            queryParams.Add("@id", deviceId);
            JArray foundDevices = await _docDbOperations.QueryDocuments("SELECT VALUE root FROM root WHERE (root.DeviceProperties.DeviceID = @id)", queryParams);

            if (foundDevices != null && foundDevices.Count > 0)
            {
                result = foundDevices.Children().ElementAt(0);
            }

            return result;
        }

        /// <summary>
        /// Updates an existing device in the DocumentDB
        /// Throws a DeviceNotRegisteredException is the device does not already exist in the DocumentDB
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<dynamic> UpdateDeviceAsync(dynamic device)
        {
            string deviceId = DeviceSchemaHelper.GetDeviceID(device);

            dynamic existingDevice = await GetDeviceAsync(deviceId);

            if (existingDevice == null)
            {
                throw new DeviceNotRegisteredException(deviceId);
            }

            string incomingRid = DeviceSchemaHelper.GetDocDbRid(device);

            if (string.IsNullOrWhiteSpace(incomingRid))
            {
                // copy the existing _rid onto the incoming data if needed
                var existingRid = DeviceSchemaHelper.GetDocDbRid(existingDevice);
                if (string.IsNullOrWhiteSpace(existingRid))
                {
                    throw new InvalidOperationException("Could not find _rid property on existing device");
                }
                device._rid = existingRid;
            }

            string incomingId = DeviceSchemaHelper.GetDocDbId(device);

            if (string.IsNullOrWhiteSpace(incomingId))
            {
                // copy the existing id onto the incoming data if needed
                var existingId = DeviceSchemaHelper.GetDocDbId(existingDevice);
                if (string.IsNullOrWhiteSpace(existingId))
                {
                    throw new InvalidOperationException("Could not find id property on existing device");
                }
                device.id = existingId;
            }

            DeviceSchemaHelper.UpdateUpdatedTime(device);

            device = await _docDbOperations.UpdateDocumentAsync(device, incomingRid);

            return device;
        }

    }
}
