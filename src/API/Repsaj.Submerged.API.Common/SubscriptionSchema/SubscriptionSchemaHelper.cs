using Newtonsoft.Json.Linq;
using Repsaj.Submerged.Common.Exceptions;
using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Common.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.SubscriptionSchema
{
    public static class SubscriptionSchemaHelper
    {
        /// <summary>
        /// Gets a SubscriptionProperties instance from a Subscription.
        /// </summary>
        /// <param name="subscription">
        /// The Subscription from which to extract a SubscriptionProperties instance.
        /// </param>
        /// <returns>
        /// A SubscriptionProperties instance, extracted from <paramref name="subscription"/>.
        /// </returns>
        public static SubscriptionPropertiesModel GetSubscriptionProperties(SubscriptionModel subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            var props = subscription.SubscriptionProperties;

            if (props == null)
            {
                throw new SubscriptionRequiredPropertyNotFoundException("'SubscriptionProperties' property is missing");
            }

            return props;
        }

        /// <summary>
        /// Gets a Subscription instance's Subscription ID.
        /// </summary>
        /// <param name="subscription">
        /// The Subscription instance from which to extract a Subscription ID.
        /// </param>
        /// <returns>
        /// The Subscription ID, extracted from <paramref name="subscription" />.
        /// </returns>
        public static Guid GetSubscriptionID(SubscriptionModel subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            var props = GetSubscriptionProperties(subscription);

            Guid subscriptionID = props.SubscriptionID;

            if (subscriptionID == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'SubscriptionID' property is missing");
            }

            return subscriptionID;
        }

        /// <summary>
        /// Gets a Subscription instance's Subscription ID.
        /// </summary>
        /// <param name="subscription">
        /// The Subscription instance from which to extract a Subscription ID.
        /// </param>
        /// <returns>
        /// The Subscription ID, extracted from <paramref name="subscription" />.
        /// </returns>
        public static string GetSubscriptionUser(SubscriptionModel subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            var props = GetSubscriptionProperties(subscription);

            string subscriptionUser = props.User;

            if (subscriptionUser == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'User' property is missing");
            }

            return subscriptionUser;
        }

        /// <summary>
        /// Extracts an Updated Time value from a Device instance.
        /// </summary>
        /// <param name="device">
        /// The Subscription instance from which to extract an Updated Time value.
        /// </param>
        /// <returns>
        /// The Updated Time value, extracted from <paramref name="subscription" />, or
        /// null if it is null or does not exist.
        /// </returns>
        public static DateTime? GetUpdatedTime(SubscriptionModel subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            var props = GetSubscriptionProperties(subscription);

            // note that since null is a valid value, don't try to test if the actual UpdateTime is there

            return props.UpdatedTime;
        }

        /// <summary>
        /// Set the current time (in UTC) to the device's UpdatedTime Device Property
        /// </summary>
        /// <param name="subscription"></param>
        public static void UpdateUpdatedTime(SubscriptionModel subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            var props = GetSubscriptionProperties(subscription);

            props.UpdatedTime = DateTime.UtcNow;
        }       
    }
}
