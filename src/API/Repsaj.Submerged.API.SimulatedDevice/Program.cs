using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace Repsaj.Submerged.API.SimulatedDevice
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = ConfigurationManager.AppSettings["iotHubUri"];
        static string deviceKey = ConfigurationManager.AppSettings["deviceKey"];
        static string deviceId = ConfigurationManager.AppSettings["deviceId"];
        static JsonSerializerSettings _settings;

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));

            // use camlCasing for json objects
            _settings = new JsonSerializerSettings();
            _settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            Task.Run(() => StartCommsAsync());

            Console.ReadLine();
        }

        private static async void StartCommsAsync()
        {
            await SendDeviceInfoAsync();
            SendDeviceToCloudMessagesAsync();

            ReceiveC2dAsync();
        }

        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }

        private static async void SendDeviceToCloudInteractiveMessagesAsync()
        {
            while (true)
            {
                var interactiveMessageString = "Alert message!";
                var interactiveMessage = new Message(Encoding.ASCII.GetBytes(interactiveMessageString));
                interactiveMessage.Properties["messageType"] = "interactive";
                interactiveMessage.MessageId = Guid.NewGuid().ToString();

                await deviceClient.SendEventAsync(interactiveMessage);
                Console.WriteLine("{0} > Sending interactive message: {1}", DateTime.Now, interactiveMessageString);

                Thread.Sleep(50000);
            }
        }

        private static async Task SendDeviceInfoAsync()
        {
            string deviceInfo = $"{{\r\n  \"DeviceProperties\": {{\r\n    \"DeviceID\": \"{deviceId}\",\r\n    \"HubEnabledState\": true,\r\n    \"CreatedTime\": \"2016-05-15T12:54:47.9434001Z\",\r\n    \"DeviceState\": \"normal\",\r\n    \"UpdatedTime\": null,\r\n    \"Manufacturer\": \"Repsaj Inc.\"\r\n  }},\r\n  \"Commands\": [],\r\n  \"CommandHistory\": [],\r\n  \"ModuleStatus\": [],\r\n  \"IsSimulatedDevice\": false,\r\n  \"ObjectType\": \"DeviceInfo\",\r\n  \"Version\": \"1.0\"\r\n}}";
            await SendMessage(deviceInfo);
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double avgTemperature1 = 20; 
            double avgTemperature2 = 19; 
            double avgpH = 7;
            Random rand = new Random();

            while (true)
            {
                double currentTemp1 = avgTemperature1 + rand.NextDouble() * 5 - 2.5;
                double currentTemp2 = avgTemperature2 + rand.NextDouble() * 5 - 2.5;
                double currentPH = avgpH + rand.NextDouble() * 1.2 - 0.6;

                JObject data = new JObject();
                data.Add("objectType", "Telemetry");
                data.Add("deviceId", deviceId);

                dynamic telemetryDataPoint = new DeviceTelemetryModel()
                {
                    temperature1 = currentTemp1,
                    temperature2 = currentTemp2,
                    pH = currentPH,
                    leakDetected = false,
                    leakSensors = ""
                };

                // merge the telemetry data into our data payload
                data.Merge(JObject.FromObject(telemetryDataPoint));
                var jsonObject = JsonConvert.SerializeObject(data, _settings);
                await SendMessage(jsonObject);
                Thread.Sleep(1000);
            }
        }

        private static async Task SendMessage(string jsonObject)
        {
            var bytes = Encoding.UTF8.GetBytes(jsonObject);

            var message = new Microsoft.Azure.Devices.Client.Message(bytes);
            message.Properties["EventId"] = (Guid.NewGuid()).ToString();

            await deviceClient.SendEventAsync(message);
            Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, jsonObject);
            Debug.WriteLine(jsonObject);

        }
    }
}
