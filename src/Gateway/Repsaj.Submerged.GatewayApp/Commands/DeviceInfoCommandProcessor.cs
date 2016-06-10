using Newtonsoft.Json.Linq;
using RemoteArduino.Commands;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteArduino.Commands
{
    class DeviceInfoCommandProcessor : ICommandProcessor
    {
        private const string UPDATE_INFO = "UpdateInfo";
        private readonly IConfigurationRepository _configRepository;

        public DeviceInfoCommandProcessor(IConfigurationRepository configurationRepository)
        {
            _configRepository = configurationRepository;
        }

        public async Task<CommandProcessingResult> ProcessCommand(DeserializableCommand command)
        {
            if (command.CommandName == UPDATE_INFO)
            {
                try
                {
                    dynamic parameters = command.Command.Parameters;

                    JArray modulesArray = parameters.Modules;
                    List<ModuleConfigurationModel> modules = new List<ModuleConfigurationModel>();

                    foreach (JObject moduleObj in modulesArray)
                    {
                        ModuleConfigurationModel model = new ModuleConfigurationModel();

                        model.ConnectionString = (string)moduleObj["ConnectionString"];
                        model.ModuleType = (string)moduleObj["ModuleType"];
                        model.Name = (string)moduleObj["Name"];

                        modules.Add(model);
                    }

                    await _configRepository.SaveModuleConfiguration(modules.ToArray());

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
