using Repsaj.Submerged.Common.SubscriptionSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public interface ISubscriptionRepository
    {
        /// <summary>
        /// Adds a subscription asynchronously.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
        Task<SubscriptionModel> AddSubscriptionAsync(SubscriptionModel subscription);

        /// <summary>
        /// Gets a subscription asynchronously.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        Task<SubscriptionModel> GetSubscriptionAsync(Guid subscriptionId, string subscriptionUser);

        /// <summary>
        /// Gets a subscription asynchronously.
        /// </summary>
        /// <param name="subscriptionUser">The subscription user.</param>
        /// <returns></returns>
        Task<SubscriptionModel> GetSubscriptionAsync(string subscriptionUser);

        Task<SubscriptionModel> GetSubscriptionByDeviceId(string deviceId, string subscriptionUser, bool skipValidation = false);

        /// <summary>
        /// Updates a subscription asynchronously.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
        Task<SubscriptionModel> UpdateSubscriptionAsync(SubscriptionModel subscription, string subscriptionUser, bool skipValidation = false);

        Task<DeviceModel> GetDeviceAsync(string deviceId);

        /// <summary>
        /// Deletes a subscription asynchronously
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        Task DeleteSubscriptionAsync(SubscriptionModel subscription);
    }
}
