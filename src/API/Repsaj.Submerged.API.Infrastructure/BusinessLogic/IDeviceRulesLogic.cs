using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public interface IDeviceRulesLogic
    {
        Task<List<DeviceRule>> GetDeviceRulesAsync(string deviceId);
        Task<List<DeviceRule>> GetAllRulesAsync();
        Task<DeviceRule> GetNewRuleAsync(string deviceId);
        Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule);
        Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(string deviceId, string ruleId);
        Task<bool> RemoveAllRulesForDeviceAsync(string deviceId);
    }
}
