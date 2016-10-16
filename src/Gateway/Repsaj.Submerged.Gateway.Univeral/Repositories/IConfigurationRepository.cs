using Repsaj.Submerged.GatewayApp.Universal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Repositories
{
    public interface IConfigurationRepository
    {
        //Task<IEnumerable<ModuleConfigurationModel>> GetModuleConfiguration();
        //Task SaveModuleConfiguration(ModuleConfigurationModel[] moduleConfiguration);

        Task<ConnectionInformationModel> GetConnectionInformationModelAsync();
        Task SaveConnectionInformationModelAsync(ConnectionInformationModel model);

        Task<DeviceModel> GetDeviceModelAsync();
        Task SaveDeviceModelAsync(DeviceModel model);

        string GetConnectionInfoPath();
    }
}
