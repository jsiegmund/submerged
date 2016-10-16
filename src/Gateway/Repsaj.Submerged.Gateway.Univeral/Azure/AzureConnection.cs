using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Repsaj.Submerged.Gateway.Common.Log;
using Repsaj.Submerged.GatewayApp.Universal.Commands;
using Repsaj.Submerged.GatewayApp.Universal.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace Repsaj.Submerged.GatewayApp.Universal.Azure
{
    class AzureConnection : IAzureConnection
    {
        public event ICommandReceived CommandReceived;
        public event Action Connected;
        public event Action Disconnected;

        string _deviceConnectionString;
        JsonSerializerSettings _settings;
        DeviceClient _deviceClient;

        bool _connected = false;

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
            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString, TransportType.Amqp);
        }
        
        private async Task TestConnection()
        {
            try
            {
                var message = new Microsoft.Azure.Devices.Client.Message();

                await _deviceClient.SendEventAsync(message);

                SetConnected();
            }
            catch (Exception ex) when (ex.Message.Contains("UnauthorizedException"))
            {
                SetDisconnected();
                throw new DeviceNotAuthorizedException();
            }
            catch (Exception ex) when (ex.Message.Contains("DeviceNotFound"))
            {
                SetDisconnected();
                throw new DeviceNotFoundException();
            }
            catch (Exception ex)
            {
                SetDisconnected();
                throw ex;
            }
        }
        public async Task Init()
        {
            try
            {
                await TestConnection();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => { ListenToCloudMessagesAsync(); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error($"Could not connect to Azure because: {ex}");
                throw;
            }
        }

        private void SetDisconnected()
        {
            if (_connected)
            {
                _connected = false;
                Disconnected?.Invoke();
            }
        }

        private void SetConnected()
        {
            if (!_connected)
            {
                _connected = true;
                Connected?.Invoke();
            }
        }

        /// <summary>
        /// Sends an event to the IoT Hub
        /// </summary>
        /// <param name="device"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public Task<string> SendDeviceToCloudMessageAsync(JObject eventData)
        {
            string payload = eventData.ToString();
            return SendDeviceToCloudMessageAsync(payload);
        }

        public async Task<string> SendDeviceToCloudMessageAsync(string payload)
        {
            var eventId = Guid.NewGuid();

            byte[] bytes;

            try
            {
                bytes = Encoding.UTF8.GetBytes(payload);

                var message = new Microsoft.Azure.Devices.Client.Message(bytes);
                message.Properties["EventId"] = eventId.ToString();

                Debug.WriteLine("{0} > Sending IoT hub message: {1}", DateTime.UtcNow, payload);
                await _deviceClient.SendEventAsync(message);

                SetConnected();

                return payload;
            }
            catch (Exception ex)
            {
                SetDisconnected();
                throw new Exception(String.Format("Failed sending message ({0}) to Azure: {1}", payload, ex.ToString()));
            }
        }

        private async Task ListenToCloudMessagesAsync()
        {
            while (true)
            {
                Message receivedMessage = null;
                await Task.Delay(1000);

                try
                {
                   receivedMessage = await _deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failure occurred whilst waiting for a cloud to device message: " + ex);
                    // when something happens in the transport; reboot the client
                    _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString);
                    SetDisconnected();
                }

                if (receivedMessage == null) continue;
                Debug.WriteLine($"Received a message from Azure!");

                try
                {
                    DeserializableCommand command = new DeserializableCommand(receivedMessage);
                    await CommandReceived(command);

                    SetConnected();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Could not process the command received: " + ex);
                    SetDisconnected();
                }
                finally
                {
                    await _deviceClient.CompleteAsync(receivedMessage);
                    SetConnected();
                }

            }
        }        
    }
}
