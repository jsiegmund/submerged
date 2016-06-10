using Repsaj.Submerged.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public interface IIotHubRepository
    {
        Task SendCommand(string deviceId, dynamic command);
        Task<dynamic> AddDeviceAsync(dynamic device, SecurityKeys securityKeys);
    }
}
