using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Exceptions
{

    [Serializable]
    public class SubscriptionUnknownException : Exception
    {
        public SubscriptionUnknownException() { }
        public SubscriptionUnknownException(string message) : base(message) { }
        public SubscriptionUnknownException(string message, Exception inner) : base(message, inner) { }
        protected SubscriptionUnknownException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
