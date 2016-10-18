using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Helpers
{
    public class FixedSizeQueue<T>: ConcurrentQueue<T>
    {
        int _capacity;

        public FixedSizeQueue(int capacity): base()
        {
            this._capacity = capacity;
        }

        public new void Enqueue(T item)
        {
            // enqueue a new item
            base.Enqueue(item);

            // dequeue items as long as the size of the queue is over the target capacity
            T overflow;
            while (base.Count > _capacity && base.TryDequeue(out overflow)) ;
        }
    }
}
