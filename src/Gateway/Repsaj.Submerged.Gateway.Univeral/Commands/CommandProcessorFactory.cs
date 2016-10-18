using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Commands
{
    public class CommandProcessorFactory : ICommandProcessorFactory
    {
        SwitchRelayCommandProcessor _switchRelayProcessor;
        DeviceInfoCommandProcessor _deviceInfoProcessor;
        ModuleCommandProcessor _moduleCommandProcessor;

        public CommandProcessorFactory(SwitchRelayCommandProcessor switchRelayProcessor, DeviceInfoCommandProcessor deviceInfoProcessor,
            ModuleCommandProcessor moduleCommandProcessor)
        {
            _switchRelayProcessor = switchRelayProcessor;
            _deviceInfoProcessor = deviceInfoProcessor;
            _moduleCommandProcessor = moduleCommandProcessor; 
        }

        public ICommandProcessor FindCommandProcessor(DeserializableCommand command)
        {
            return FindCommandProcessor(command.CommandName);
        }

        public ICommandProcessor FindCommandProcessor(string commandName)
        {
            if (commandName == CommandNames.SWITCH_RELAY)
                return _switchRelayProcessor;
            else if (commandName == CommandNames.UPDATE_INFO)
                return _deviceInfoProcessor;
            else if (commandName == CommandNames.MODULE_COMMAND)
                return _moduleCommandProcessor;
            else
                return null;
        }
    }
}
