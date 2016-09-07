using Repsaj.Submerged.Common.Configurations;
using System;
using System.Net;
using Microsoft.Azure.Devices;
using Repsaj.Submerged.Common.Helpers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Repsaj.Submerged.Common.DeviceSchema;
using Repsaj.Submerged.Common.Models;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public class IotHubRepository : IIotHubRepository, IDisposable
    {
        readonly string _iotHubConnectionString;
        readonly RegistryManager _deviceManager;
        bool _disposed = false;

        public IotHubRepository(IConfigurationProvider configProvider)
        {
            // Temporary code to bypass https cert validation till DNS on IotHub is configured
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            _iotHubConnectionString = configProvider.GetConfigurationSettingValue("iotHub.ConnectionString");
            _deviceManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
        }

        /// <summary>
        /// Adds the provided device to the IoT hub with the provided security keys
        /// </summary>
        /// <param name="device"></param>
        /// <param name="securityKeys"></param>
        /// <returns></returns>
        public async Task<dynamic> AddDeviceAsync(dynamic device, SecurityKeys securityKeys)
        {
            Device iotHubDevice = new Device(DeviceSchemaHelper.GetDeviceID(device));

            var authentication = new AuthenticationMechanism
            {
                SymmetricKey = new SymmetricKey
                {
                    PrimaryKey = securityKeys.PrimaryKey,
                    SecondaryKey = securityKeys.SecondaryKey
                }
            };

            iotHubDevice.Authentication = authentication;

            await AzureRetryHelper.OperationWithBasicRetryAsync<Device>(async () =>
                await _deviceManager.AddDeviceAsync(iotHubDevice));

            return device;
        }

        /// <summary>
        /// Deletes the device from the IoT Hub device registry
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task DeleteDeviceAsync(dynamic device)
        {
            Device iotHubDevice = await _deviceManager.GetDeviceAsync(DeviceSchemaHelper.GetDeviceID(device));

            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await _deviceManager.RemoveDeviceAsync(iotHubDevice));

        }

        /// <summary>
        /// Sends a fire and forget command to the device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task SendCommand(string deviceId, dynamic command)
        {
            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(_iotHubConnectionString);

            byte[] commandAsBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));
            var notificationMessage = new Message(commandAsBytes);

            notificationMessage.Ack = DeliveryAcknowledgement.Full;
            notificationMessage.MessageId = command.MessageId;

            await AzureRetryHelper.OperationWithBasicRetryAsync(async () =>
                await serviceClient.SendAsync(deviceId, notificationMessage));

            await serviceClient.CloseAsync();
        }


        /// <summary>
        /// Implement the IDisposable interface in order to close the device manager
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_deviceManager != null)
                {
                    _deviceManager.CloseAsync().Wait();
                }
            }

            _disposed = true;
        }

        ~IotHubRepository()
        {
            Dispose(false);
        }
    }
}
