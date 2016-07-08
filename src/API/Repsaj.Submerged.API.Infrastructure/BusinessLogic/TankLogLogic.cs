using Repsaj.Submerged.Infrastructure.Models;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Infrastructure.Exceptions;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public class TankLogLogic : ITankLogLogic
    {
        private readonly ITankLogRepository _tankLogRepository;

        public TankLogLogic(ITankLogRepository tankLogRepository)
        {
            _tankLogRepository = tankLogRepository;
        }

        public async Task<List<TankLog>> GetTankLogAsync(Guid tankId)
        {
            return await _tankLogRepository.GetTankLogAsync(tankId);
        }

        public async Task<TableStorageResponse<TankLog>> SaveTankLogAsync(TankLog logLine)
        {
            if (String.IsNullOrEmpty(logLine.Title))
                throw new SubscriptionValidationException(Strings.ValidationTitleEmpty);
            if (String.IsNullOrEmpty(logLine.Description))
                throw new SubscriptionValidationException(Strings.ValidationDescriptionEmpty);
            if (String.IsNullOrEmpty(logLine.LogType))
                throw new SubscriptionValidationException(Strings.ValidationLogTypeEmpty);

            return await _tankLogRepository.SaveTankLogAsync(logLine); 
        }

        public async Task<TableStorageResponse<TankLog>> DeleteTankLogAsync(Guid tankId, Guid logId)
        {
            TankLog found = await _tankLogRepository.GetTankLogAsync(tankId, logId);
            if (found == null)
            {
                var response = new TableStorageResponse<TankLog>();
                response.Entity = found;
                response.Status = TableStorageResponseStatus.NotFound;

                return response;
            }

            return await _tankLogRepository.DeleteTankLogAsync(found);
        }

        public async Task<TankLog> GetTankLogAsync(Guid tankId, Guid logId)
        {
            return await _tankLogRepository.GetTankLogAsync(tankId, logId);
        }
    }
}
