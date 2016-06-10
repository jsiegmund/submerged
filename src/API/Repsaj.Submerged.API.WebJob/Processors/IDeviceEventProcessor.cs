using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repsaj.Submerged.WebJob.Processors
{
    public interface IDeviceEventProcessor
    {
        void Start();

        void Start(CancellationToken token);

        void Stop();
    }
}
