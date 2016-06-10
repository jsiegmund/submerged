using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteArduino.Commands
{
    interface ICommandProcessor
    {
        Task<CommandProcessingResult> ProcessCommand(DeserializableCommand command);
    }
}
