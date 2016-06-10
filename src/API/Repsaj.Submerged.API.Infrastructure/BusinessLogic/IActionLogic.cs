using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public interface IActionLogic
    {
        Task<List<string>> GetAllActionIdsAsync();
        Task<bool> SendNotificationAsync(string title, string message);
        Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue);
    }
}
