using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteArduino.Commands
{
    /// <summary>
    /// The supported command processing results.
    /// </summary>
    public enum CommandProcessingResult
    {
        Success = 0,
        RetryLater,
        CannotComplete
    }
}
