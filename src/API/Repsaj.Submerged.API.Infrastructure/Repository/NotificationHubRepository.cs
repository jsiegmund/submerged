using Microsoft.Azure.NotificationHubs;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public class NotificationHubRepository : INotificationHubRepository
    {
        // Initialize the Notification Hub
        NotificationHubClient _hub;

        public NotificationHubRepository(IConfigurationProvider configurationProvider)
        {
            string notificationHubConnection = configurationProvider.GetConfigurationSettingValue("MS_NotificationHubConnectionString");
            string notificationHubName = configurationProvider.GetConfigurationSettingValue("MS_NotificationHubName");

            _hub = NotificationHubClient.CreateClientFromConnectionString(notificationHubConnection, notificationHubName);
        }

        public async Task CreateOrUpdateInstallationAsync(string installationId, string registrationId, IEnumerable<string> tags)
        {
            Installation installation = new Installation();
            installation.InstallationId = installationId;
            installation.PushChannel = registrationId;
            installation.Tags = tags.ToArray();
            installation.Platform = NotificationPlatform.Gcm;

            await _hub.CreateOrUpdateInstallationAsync(installation);
        }
    }
}
