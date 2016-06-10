using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.WebJob.Processors
{
    public interface IActionEventProcessor
    {
        void Start();
        void Stop();
    }
}
