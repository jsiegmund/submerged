using Repsaj.Submerged.Gateway.Common.Log;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Commands
{
    public class ModuleCommandProcessor : ICommandProcessor
    {
        IModuleConnectionFactory _moduleConnectionFactory;

        public ModuleCommandProcessor(IModuleConnectionFactory connectionFactory)
        {
            this._moduleConnectionFactory = connectionFactory;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<CommandProcessingResult> ProcessCommand(DeserializableCommand command)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (command.CommandName == CommandNames.MODULE_COMMAND)
            {
                try
                {
                    dynamic parameters = command.Command.Parameters;
                    string moduleName = parameters.ModuleName;
                    IModuleConnection module = _moduleConnectionFactory.GetModuleConnection(moduleName);

                    // if the module could not be found or is not connected, return cannot complete
                    if (module == null || module.ModuleStatus != ModuleConnectionStatus.Connected)
                        return CommandProcessingResult.CannotComplete;

                    // execute the command, switch the relay
                    await module.ProcessCommand(parameters);

                    return CommandProcessingResult.Success;
                }
                catch (Exception ex)
                {
                    LogEventSource.Log.Error($"Could not process command received from Azure: {ex}. Command with name '{command.CommandName}' was: {command.Command}");
                    return CommandProcessingResult.CannotComplete;
                }
            }

            return CommandProcessingResult.CannotComplete;
        }
    }
}
