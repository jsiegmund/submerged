using Microsoft.Azure.NotificationHubs;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    /// <summary>
    /// Repository storing available actions for rules.
    /// </summary>
    public class ActionRepository : IActionRepository
    {
        IConfigurationProvider _configuration;

        public ActionRepository(IConfigurationProvider configurationProvider)
        {
            _configuration = configurationProvider;
        }

        // Currently this list is not editable in the app
        private List<string> _actionIds = new List<string>()
        {
            "Send Message",
            "Raise Alarm"
        };

        public async Task<List<string>> GetAllActionIdsAsync()
        {
            return await Task.Run(() => { return _actionIds; });
        }

        public async Task<bool> SendNotificationAsync(string title, string message)
        {
            string notificationHubConnection = _configuration.GetConfigurationSettingValue("MS_NotificationHubConnectionString");
            string notificationHubName = _configuration.GetConfigurationSettingValue("MS_NotificationHubName");

            // Create a new Notification Hub client.
            NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(notificationHubConnection, notificationHubName);

            // Construct the android notification payload
            dynamic androidNotificationPayload = new System.Dynamic.ExpandoObject();
            androidNotificationPayload.data = new System.Dynamic.ExpandoObject();
            androidNotificationPayload.data.title = title;
            androidNotificationPayload.data.message = message;

            // conver the payload to a string which we'll need to send 
            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(androidNotificationPayload);

            try
            {
                // Send the push notification and log the results.
                await hub.SendGcmNativeNotificationAsync(jsonPayload);
                return true;
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("Fault when sending notification: {0}", ex);
            }

            return false;
        }

        public async Task<bool> ExecuteLogicAppAsync(string actionId, string deviceId, string measurementName, double measuredValue)
        {
            Debug.WriteLine("ExecuteLogicAppAsync is not yet implemented");

            await Task.Run(() => { });
            return false;
        }
    }
}
