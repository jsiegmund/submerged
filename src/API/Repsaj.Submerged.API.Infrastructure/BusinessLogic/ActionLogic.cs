using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public class ActionLogic : IActionLogic
    {
        private readonly IActionRepository _actionRepository;

        public ActionLogic(IActionRepository actionRepository)
        {
            _actionRepository = actionRepository;
        }

        public async Task<List<string>> GetAllActionIdsAsync()
        {
            return await _actionRepository.GetAllActionIdsAsync();
        }

        public async Task<bool> SendNotificationAsync(string title, string message)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("title cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("message cannot be null or whitespace");
            }

            return await _actionRepository.SendNotificationAsync(title, message);
        }

        public async Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                throw new ArgumentException("actionId cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentException("deviceId cannot be null or whitespace");
            }

            // check that the actionId is valid!
            var validActionIds = await GetAllActionIdsAsync();
            if (!validActionIds.Contains(actionId))
            {
                throw new ArgumentException("actionId must be a valid ActionId value");
            }

            return await _actionRepository.ExecuteLogicAppAsync(actionId, deviceId, measurementName, measuredValue);
        }
    }
}
