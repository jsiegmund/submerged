using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Infrastructure.Models;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public interface ITankLogRepository
    {
        Task<List<TankLog>> GetTankLogAsync(Guid tankId);
        Task<TableStorageResponse<TankLog>> SaveTankLogAsync(TankLog logLine);
        Task<TankLog> GetTankLogAsync(Guid tankId, Guid logId);
        Task<TableStorageResponse<TankLog>> DeleteTankLogAsync(TankLog found);
    }
}
