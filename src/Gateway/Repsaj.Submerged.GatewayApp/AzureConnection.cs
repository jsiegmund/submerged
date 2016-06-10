using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RemoteArduino.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp
{
    class AzureConnection
    {
        public delegate Task ICommandReceived(DeserializableCommand command);

        public event ICommandReceived CommandReceived;

        string _deviceConnectionString;
        JsonSerializerSettings _settings;
        DeviceClient _deviceClient;

        public AzureConnection(string IoTHubHostname, string deviceId, string deviceKey)
        {
            string deviceConnectionString = $"HostName={IoTHubHostname};DeviceId={deviceId};SharedAccessKey={deviceKey}";

            // Initialize the device client object which is used to connect to Azure IoT hub
            // Create the IoT Hub Device Client instance
            _deviceConnectionString = deviceConnectionString;

            // use camlCasing for json objects
            _settings = new JsonSerializerSettings();
            _settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // create the device client for Azure
            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString);

            // start listening for Cloud 2 Device messages 
            ReceiveC2dAsync();
        }

        /// <summary>
        /// Sends an event to the IoT Hub
        /// </summary>
        /// <param name="device"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public Task<string> SendDeviceToCloudMessagesAsync(JObject eventData)
        {
            var eventId = Guid.NewGuid();
            return SendDeviceToCloudMessagesAsync(eventId, eventData);
        }

        private async Task<string> SendDeviceToCloudMessagesAsync(Guid eventId, JObject eventData)
        {
            byte[] bytes;
            string jsonObject = string.Empty;

            try
            {
                jsonObject = eventData.ToString();
                bytes = Encoding.UTF8.GetBytes(jsonObject);

                var message = new Microsoft.Azure.Devices.Client.Message(bytes);
                message.Properties["EventId"] = eventId.ToString();

                Debug.WriteLine("{0} > Sending IoT hub message: {1}", DateTime.UtcNow, jsonObject);
                await _deviceClient.SendEventAsync(message);

                return jsonObject;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed sending message ({0}) to Azure: {1}", jsonObject, ex.ToString()));
            }
        }

        private async void ReceiveC2dAsync()
        {
            while (true)
            {
                Message receivedMessage = null;

                try
                {
                    receivedMessage = await _deviceClient.ReceiveAsync();
                }
                catch (Exception ex)
                {
                    // when something happens in the transport; reboot the client
                    _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString);
                }

                if (receivedMessage == null) continue;

                try
                {
                    DeserializableCommand command = new DeserializableCommand(receivedMessage);
                    await CommandReceived(command);

                    await _deviceClient.CompleteAsync(receivedMessage);
                }
                catch (Exception ex)
                {

                }

            }
        }        
    }
}
