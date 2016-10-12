using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Commands
{
    public class DeviceInfoCommandProcessor : ICommandProcessor
    {
        public event Action<DeviceModel> DeviceModelChanged;
        private readonly IConfigurationRepository _configRepository;

        public DeviceInfoCommandProcessor(IConfigurationRepository configurationRepository)
        {
            _configRepository = configurationRepository;
        }

        public async Task<CommandProcessingResult> ProcessCommand(DeserializableCommand command)
        {
            if (command.CommandName == CommandNames.UPDATE_INFO)
            {
                try
                {
                    JObject deviceObject = (JObject)command.Command.Parameters;
                    DeviceModel deviceModel = deviceObject.ToObject<DeviceModel>();
                    await _configRepository.SaveDeviceModel(deviceModel);

                    DeviceModelChanged?.Invoke(deviceModel);

                    return CommandProcessingResult.Success;
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
