using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public interface IDeviceRulesRepository
    {
        Task<List<DeviceRule>> GetAllRulesAsync();
        Task<List<DeviceRule>> GetAllRulesForDeviceAsync(string deviceId);
        Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId);
        Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule);
        Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(DeviceRule ruleToDelete);
        Task OverrideRulesForDevice(string deviceId, bool enabled);
    }
}
