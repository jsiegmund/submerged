using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Exceptions
{

    [Serializable]
    public class SubscriptionExistsException : Exception
    {
        public SubscriptionExistsException() { }
        public SubscriptionExistsException(string message) : base(message) { }
        public SubscriptionExistsException(string message, Exception inner) : base(message, inner) { }
        protected SubscriptionExistsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
