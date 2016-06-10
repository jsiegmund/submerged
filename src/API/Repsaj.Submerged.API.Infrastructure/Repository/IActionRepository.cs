using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    /// <summary>
    /// Represents a repository for actions in response to rules (actions are currently logic apps)
    /// </summary>
    public interface IActionRepository
    {
        Task<List<string>> GetAllActionIdsAsync();

        Task<bool> SendNotificationAsync(string title, string message);

        Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue);
    }
}
