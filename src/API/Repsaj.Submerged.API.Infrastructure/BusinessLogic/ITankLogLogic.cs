using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Common.SubscriptionSchema;
using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public interface ITankLogLogic
    {
        Task<List<TankLog>> GetTankLogAsync(Guid tankId);
        Task<TableStorageResponse<TankLog>> SaveTankLogAsync(TankLog logLine);
        Task<TankLog> GetTankLogAsync(Guid tankId, Guid logId);
        Task<TableStorageResponse<TankLog>> DeleteTankLogAsync(Guid tankId, Guid logId);
    }
}
