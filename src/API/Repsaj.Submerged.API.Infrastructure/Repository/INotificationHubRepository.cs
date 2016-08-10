using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public interface INotificationHubRepository
    {
        Task CreateOrUpdateInstallationAsync(string installationId, string registrationId, IEnumerable<string> tags);
    }
}
