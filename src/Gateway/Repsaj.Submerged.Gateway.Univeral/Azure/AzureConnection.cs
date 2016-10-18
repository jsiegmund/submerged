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
using System.Threading;
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

        bool _connecting = false;
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
        }

        private async Task<bool> TestConnection()
        {
            try
            {
                // send out an empty test message to see whether we have a connection
                var message = new Microsoft.Azure.Devices.Client.Message();
                await _deviceClient.SendEventAsync(message);

                SetConnected();
                return true;
            }
            catch (Exception ex) when (ex.Message.Contains("UnauthorizedException"))
            {
                SetDisconnected();
                return false;
            }
            catch (Exception ex) when (ex.Message.Contains("DeviceNotFound"))
            {
                SetDisconnected();
                return false;
            }
            catch (Exception ex)
            {
                SetDisconnected();
                return false;
            }
        }
        public async Task Init()
        {
            // create the device client for Azure
            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString, TransportType.Amqp);

            if (await TestConnection())
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => { ListenToCloudMessagesAsync(); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private void SetDisconnected()
        {
            if (_connected)
            {
                _connected = false;
                Disconnected?.Invoke();
            }

            if (_deviceClient != null)
            {
                _deviceClient.Dispose();
                _deviceClient = null;
            }

            if (!_connecting)
            {
                _connecting = true;

                Task.Run(async () =>
                {
                    Debug.WriteLine("Starting a delay task to reboot the Azure connection in 5 minutes");

                    await Task.Delay(new TimeSpan(0, 0, 10));
                    _connecting = false;

                    Debug.WriteLine("Trying to reconnect to Azure");
                    await Init();

                });
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
        public Task<bool> SendDeviceToCloudMessageAsync(JObject eventData)
        {
            string payload = eventData.ToString();
            return SendDeviceToCloudMessageAsync(payload);
        }

        public async Task<bool> SendDeviceToCloudMessageAsync(string payload)
        {
            if (!_connected)
            {
                LogEventSource.Log.Warn("Did not send message to Azure because the connection is down.");
                return false;
            }

            try
            {
                var eventId = Guid.NewGuid();
                byte[] bytes = Encoding.UTF8.GetBytes(payload);

                var message = new Microsoft.Azure.Devices.Client.Message(bytes);
                message.Properties["EventId"] = eventId.ToString();

                Debug.WriteLine("{0} > Sending IoT hub message: {1}", DateTime.UtcNow, payload);
                await _deviceClient.SendEventAsync(message);

                return true;
            }
            catch (Exception ex)
            {
                LogEventSource.Log.Error($"Failed sending message ({payload}) to Azure: {ex}");
                SetDisconnected();
                return false;
            }
        }

        private async Task ListenToCloudMessagesAsync()
        {
            try
            {
                while (_connected)
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
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unexpected exception listening to incoming cloud messages: " + ex.ToString());
            }
        } 
    }
}
