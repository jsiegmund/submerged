using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteArduino.Commands
{
    internal class CommandProcessorFactory : ICommandProcessorFactory
    {
        SwitchRelayCommandProcessor _switchRelayProcessor;
        DeviceInfoCommandProcessor _deviceInfoProcessor;

        public CommandProcessorFactory(SwitchRelayCommandProcessor switchRelayProcessor, DeviceInfoCommandProcessor deviceInfoProcessor)
        {
            _switchRelayProcessor = switchRelayProcessor;
            _deviceInfoProcessor = deviceInfoProcessor;
        }

        public ICommandProcessor FindCommandProcessor(DeserializableCommand command)
        {
            switch (command.CommandName)
            {
                case "SwitchRelay":
                    return _switchRelayProcessor;
                case "UpdateInfo":
                    return _deviceInfoProcessor;
                default:
                    return null;
            }
        }
    }
}
