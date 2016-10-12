using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Commands
{
    public interface ICommandProcessor
    {
        Task<CommandProcessingResult> ProcessCommand(DeserializableCommand command);
    }
}
