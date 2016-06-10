using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Exceptions
{
    /// <summary>
    /// Simple base class for device administration based exceptions
    /// </summary>
    [Serializable]
    public abstract class SubscriptionAdministrationExceptionBase : Exception
    {
        // TODO: Localize this, if neccessary.
        private const string subscriptionIdMessageFormatString = "SubscriptionId: {0}";

        public string SubscriptionId { get; set; }

        public SubscriptionAdministrationExceptionBase() : base()
        {
        }

        public SubscriptionAdministrationExceptionBase(string subscriptionId)
            : base(string.Format(CultureInfo.CurrentCulture, subscriptionIdMessageFormatString, subscriptionId))
        {
            SubscriptionId = subscriptionId;
        }

        public SubscriptionAdministrationExceptionBase(string subscriptionId, Exception innerException)
            : base(string.Format(CultureInfo.CurrentCulture, subscriptionIdMessageFormatString, subscriptionId), innerException)
        {
            SubscriptionId = subscriptionId;
        }

        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected SubscriptionAdministrationExceptionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.SubscriptionId = info.GetString("SubscriptionId");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("SubscriptionId", SubscriptionId);
            base.GetObjectData(info, context);
        }
    }
}
