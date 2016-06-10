using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Newtonsoft.Json.Linq;

namespace Repsaj.Submerged.GatewayApp.Universal.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly IStorageRepository _storageRepository;

        private readonly string FILE_MODULE = "moduleconfig.json";
        private readonly string FILE_DEVICE = "deviceconfig.json";

        public ConfigurationRepository(IStorageRepository storageRepository)
        {
            _storageRepository = storageRepository;
        }

        public async Task<DeviceConfigurationModel> GetDeviceConfiguration()
        {
            dynamic stored = await _storageRepository.GetStoredObject(FILE_DEVICE);

            if (stored != null)
            {
                return ((JObject)stored).ToObject<DeviceConfigurationModel>();
            }

            return null;
        }

        public async Task SaveDeviceConfiguration(DeviceConfigurationModel model)
        {
            await _storageRepository.SaveObjectToStorage(model, FILE_DEVICE);
        }

        public async Task<IEnumerable<ModuleConfigurationModel>> GetModuleConfiguration()
        {
            dynamic stored = await _storageRepository.GetStoredObject(FILE_MODULE);

            if (stored != null)
            {
                List<ModuleConfigurationModel> result = new List<ModuleConfigurationModel>();

                foreach (JObject obj in (stored as JArray))
                {
                    result.Add(obj.ToObject<ModuleConfigurationModel>());
                }

                return result.ToArray();
            }
            else
                return new ModuleConfigurationModel[0];
        }

        public async Task SaveModuleConfiguration(ModuleConfigurationModel[] moduleConfiguration)
        {
            await _storageRepository.SaveObjectToStorage(moduleConfiguration, FILE_MODULE);
        }
    }
}
