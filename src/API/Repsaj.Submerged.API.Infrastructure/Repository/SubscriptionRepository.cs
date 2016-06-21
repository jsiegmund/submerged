using Newtonsoft.Json.Linq;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.Schema;
using Repsaj.Submerged.Common.SubscriptionSchema;
using Repsaj.Submerged.Common.Utility;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        // Configuration strings for use in accessing the DocumentDB, Database and DocumentCollection
        readonly string _endpointUri;
        readonly string _authorizationKey;
        readonly string _databaseId;
        readonly string _documentCollectionName;

        IDocDbOperations _docDbOperations;

        public SubscriptionRepository(IConfigurationProvider configProvider, IDocDbOperations docDbOperations,
            IDeviceRulesLogic deviceRulesLogic)
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
        }

        public async Task<SubscriptionModel> AddSubscriptionAsync(SubscriptionModel subscription)
        {
            Guid subscriptionId = SubscriptionSchemaHelper.GetSubscriptionID(subscription);
            SubscriptionModel existingDevice = await GetSubscriptionAsync(subscriptionId);

            if (existingDevice != null)
            {
                throw new SubscriptionExistsException(subscriptionId.ToString());
            }

            SubscriptionModel savedSubscription = (SubscriptionModel)(dynamic)await _docDbOperations.SaveNewDocumentAsync(subscription);

            return subscription;
        }

        private async Task<SubscriptionModel> GetSubscriptionAsync(Guid subscriptionId)
        {
            Dictionary<string, Object> queryParams = new Dictionary<string, Object>();
            queryParams.Add("@id", subscriptionId);
            var result = await _docDbOperations.Query<SubscriptionModel>("SELECT VALUE root FROM root WHERE (root.SubscriptionProperties.SubscriptionID = @id)", queryParams);

            return result.FirstOrDefault();
        }


        public async Task<SubscriptionModel> GetSubscriptionAsync(Guid subscriptionId, string subscriptionUser)
        {
            Dictionary<string, Object> queryParams = new Dictionary<string, Object>();
            queryParams.Add("@id", subscriptionId);
            queryParams.Add("@user", subscriptionUser);
            var result = await _docDbOperations.Query<SubscriptionModel>("SELECT VALUE root FROM root WHERE (root.SubscriptionProperties.SubscriptionID = @id AND root.SubscriptionProperties.User = @user)", queryParams);

            return result.FirstOrDefault();
        }

        public async Task<SubscriptionModel> GetSubscriptionAsync(string subscriptionUser)
        {
            Dictionary<string, Object> queryParams = new Dictionary<string, Object>();
            queryParams.Add("@user", subscriptionUser);
            var result = await _docDbOperations.Query<SubscriptionModel>("SELECT VALUE root FROM root WHERE (root.SubscriptionProperties.User = @user)", queryParams);

            return result.FirstOrDefault();
        }      

        public async Task<DeviceModel> GetDeviceAsync(string deviceId)
        {
            SubscriptionModel subscription = await GetSubscriptionByDeviceId(deviceId, "", true);
            return subscription.Devices.Single(d => d.DeviceProperties.DeviceID == deviceId);
        }

        public async Task<SubscriptionModel> GetSubscriptionByDeviceId(string deviceId, string subscriptionUser, bool skipValidation = false)
        {
            Dictionary<string, Object> queryParams = new Dictionary<string, Object>();
            queryParams.Add("@deviceId", deviceId);

            IEnumerable<SubscriptionModel> queryResult;

            if (skipValidation)
            {
                queryResult = await _docDbOperations.Query<SubscriptionModel>("SELECT VALUE root FROM root JOIN device IN root.Devices WHERE (device.DeviceProperties.DeviceID = @deviceId)", queryParams);
            }
            else
            {
                queryParams.Add("@user", subscriptionUser);
                queryResult = await _docDbOperations.Query<SubscriptionModel>("SELECT VALUE root FROM root JOIN device IN root.Devices WHERE (device.DeviceProperties.DeviceID = @deviceId AND root.SubscriptionProperties.User = @user)", queryParams);
            }

            if (queryResult.Count() != 1)
                throw new SubscriptionValidationException(Strings.ValidationSubscriptionUnknown);

            return queryResult.FirstOrDefault();
        }

        public async Task<SubscriptionModel> UpdateSubscriptionAsync(SubscriptionModel subscription, string subscriptionUser, bool skipValidation = false)
        {
            dynamic exisingSubscription = await GetSubscriptionAsync(subscription.SubscriptionProperties.User);

            if (exisingSubscription == null)
            {
                throw new SubscriptionUnknownException(subscription.SubscriptionProperties.User);
            }

            // ensure the subscription being updated belongs to the passed in user
            if (!skipValidation && subscription.SubscriptionProperties.User != subscriptionUser)
                throw new SubscriptionValidationException(Strings.ValidationWrongUser);

            SubscriptionSchemaHelper.UpdateUpdatedTime(subscription);
            SubscriptionModel updatedSubscription = await _docDbOperations.UpdateDocumentAsync(subscription, subscription.Id);

            return updatedSubscription;
        }
    }
}
