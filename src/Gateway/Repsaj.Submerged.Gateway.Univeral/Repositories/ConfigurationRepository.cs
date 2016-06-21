using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Repsaj.Submerged.GatewayApp.Universal.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly IStorageRepository _storageRepository;

        private readonly string FILE_CONNECTION = "connectioninfo.json";
        private readonly string FILE_DEVICE = "deviceconfig.json";

        public ConfigurationRepository(IStorageRepository storageRepository)
        {
            _storageRepository = storageRepository;
        }

        public async Task<ConnectionInformationModel> GetConnectionInformationModel()
        {
            try
            {
                dynamic stored = await _storageRepository.GetStoredObject(FILE_CONNECTION);

                if (stored != null)
                {
                    return ((JObject)stored).ToObject<ConnectionInformationModel>();
                }

            }
            catch (System.IO.FileNotFoundException)
            {
                // ignore file not found exceptions, just return null
            }

            return null;
        }

        public async Task SaveConnectionInformationModel(ConnectionInformationModel model)
        {
            await _storageRepository.SaveObjectToStorage(model, FILE_CONNECTION);
        }

        public async Task<DeviceModel> GetDeviceModel()
        {
            dynamic stored = await _storageRepository.GetStoredObject(FILE_DEVICE);

            if (stored != null)
            {
                JObject deviceModelObject = (JObject)stored;
                return deviceModelObject.ToObject<DeviceModel>();
            }
            else
                return null;
        }

        public async Task SaveDeviceModel(DeviceModel model)
        {
            await _storageRepository.SaveObjectToStorage(model, FILE_DEVICE);
        }

        public string GetConnectionInfoPath()
        {
            string storagePath = _storageRepository.GetStorageLocationPath();
            return Path.Combine(storagePath, FILE_CONNECTION);
        }
    }
}
