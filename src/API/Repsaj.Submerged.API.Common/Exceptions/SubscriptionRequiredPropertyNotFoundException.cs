using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Common.Exceptions
{

    [Serializable]
    public class SubscriptionRequiredPropertyNotFoundException : Exception
    {
        public SubscriptionRequiredPropertyNotFoundException() { }
        public SubscriptionRequiredPropertyNotFoundException(string message) : base(message) { }
        public SubscriptionRequiredPropertyNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected SubscriptionRequiredPropertyNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
