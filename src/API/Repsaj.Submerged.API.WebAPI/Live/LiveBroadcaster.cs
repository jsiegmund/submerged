using Microsoft.AspNet.SignalR;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Repsaj.Submerged.API.Hubs;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Repsaj.Submerged.API.Live
{
    public class LiveBroadcaster : ILiveBroadcaster
    {
        string _connectionString = ConfigurationManager.AppSettings["iotHub.ConnectionString"];
        string _iotHubD2cEndpoint = "messages/events";
        EventHubClient _eventHubClient;
        private IHubContext _hubs;
        private ISubscriptionLogic _subscriptionLogic;

        public LiveBroadcaster(ISubscriptionLogic subscriptionLogic)
        {
            _eventHubClient = EventHubClient.CreateFromConnectionString(_connectionString, _iotHubD2cEndpoint);
            _hubs = GlobalHost.ConnectionManager.GetHubContext<LiveHub>();
            _subscriptionLogic = subscriptionLogic;
        }

        public void Start()
        {
            var d2cPartitions = _eventHubClient.GetRuntimeInformation().PartitionIds;
            Task.WaitAll(d2cPartitions.Select(p => ReceiveMessagesFromDeviceAsync(p)).ToArray());
        }


        private async Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = _eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
            while (true)
            {
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                // convert the json object into an object
                string jsonObject = Encoding.UTF8.GetString(eventData.GetBytes());
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonObject);

                if (data.objectType == "Telemetry")
                {
                    // broadcast that object to all clients
                    _hubs.Clients.All.sendLiveData(data);

                    //JObject dataObject = (JObject)data;
                    //dataObject.Add("timestamp", DateTime.UtcNow);
                    //string deviceId = (string)dataObject.GetValue("deviceId");

                    // store the package in documentDb
                    //_subscriptionLogic.UpdateLatestTelemetryData(deviceId, data, "", true);
                }

            }
        }
    }
}