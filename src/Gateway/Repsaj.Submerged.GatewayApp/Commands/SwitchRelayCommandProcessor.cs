using RemoteArduino.Hardware;
using Repsaj.Submerged.GatewayApp;
using Repsaj.Submerged.GatewayApp.Arduino;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteArduino.Commands
{
    public class SwitchRelayCommandProcessor : ICommandProcessor
    {
        IGPIOController _gpioController;
        IModuleConnectionFactory _moduleConnectionFactory;
        private const string SWITCH_RELAY = "SwitchRelay";

        public SwitchRelayCommandProcessor(IGPIOController gpioController, IModuleConnectionFactory connectionFactory)
        {
            this._gpioController = gpioController;
            this._moduleConnectionFactory = connectionFactory;
        }

        public async Task<CommandProcessingResult> ProcessCommand(DeserializableCommand command)
        {
            if (command.CommandName == SWITCH_RELAY)
            {
                try
                {
                    dynamic parameters = command.Command.Parameters;

                    int? relayNumber = parameters.RelayNumber;
                    bool? relayState = parameters.RelayState;

                    // TODO: should include the module name in the command payload
                    string moduleName = "Cabinet Module";// parameters.ModuleName;

                    if (relayNumber == null || relayState == null)
                        return CommandProcessingResult.CannotComplete;

                    CabinetModuleConnection connection = (CabinetModuleConnection)_moduleConnectionFactory.GetModuleConnection(moduleName);
                    connection.SwitchRelay(relayNumber.Value, relayState.Value);
                }
                catch (Exception)
                {
                    return CommandProcessingResult.CannotComplete;
                }
            }

            return CommandProcessingResult.CannotComplete;
        }
    }
}
