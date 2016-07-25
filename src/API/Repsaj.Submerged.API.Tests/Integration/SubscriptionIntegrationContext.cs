using Autofac;
using Autofac.Extras.Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.SubscriptionSchema;
using Repsaj.Submerged.Common.Utility;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests.Integration
{
    public class SubscriptionIntegrationContext : IDisposable
    {
        private AutoMock _autoMock;

        private static Guid subscription_Id = new Guid("{e4b3cb3d-a36e-487c-92ef-c8a989fe919e}");
        private static string subscription_Name = "Integration Test Subscription";
        private static string subscription_Description = "Integration Test Subscription";
        private static string subscription_User = "integration-test@submerged.com";

        private ISubscriptionLogic _subscriptionLogic;

        /// <summary>
        /// This method should not be a TestMethod, but otherwise loading the external AppSettings.config
        /// file doesn't work for some reason. Until I find a better way, this does the trick.
        /// </summary>
        public void Initialize()
        {
            _autoMock = AutoMock.GetLoose();

            // inject the actual configuration provider
            _autoMock.Provide<IConfigurationProvider, ConfigurationProvider>();
            _autoMock.Provide<ISecurityKeyGenerator, SecurityKeyGenerator>();
            _autoMock.Provide<IDocDbOperations, DocDbOperations>();

            // inject the actual repositories
            _autoMock.Provide<ISubscriptionRepository, SubscriptionRepository>();
            _autoMock.Provide<IDeviceRulesRepository, DeviceRulesRepository>();

            // Logic
            _autoMock.Provide<IDeviceRulesLogic, DeviceRulesLogic>();
            _autoMock.Provide<ISubscriptionLogic, SubscriptionLogic>();

            _subscriptionLogic = _autoMock.Create<ISubscriptionLogic>();
        }

        public async Task Integration_Subscription_CreateSubscription()
        {
            var subscription = await _subscriptionLogic.CreateSubscriptionAsync(subscription_Name, subscription_Description, subscription_User, subscription_Id);

            Assert.IsNotNull(subscription);
            Assert.AreEqual(subscription_Name, subscription.SubscriptionProperties.Name);
            Assert.AreEqual(subscription_Description, subscription.SubscriptionProperties.Description);
            Assert.AreEqual(subscription_User, subscription.SubscriptionProperties.User);
        }

        public async Task Integration_Subscription_AddDevices()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);

            DeviceModel model = DeviceModel.BuildDevice(TestStatics.device_id, TestStatics.device_isSimulated);

            var device = await _subscriptionLogic.AddDeviceAsync(model, subscription_User);
            Assert.IsNotNull(device);

            subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            Assert.AreEqual(1, subscription.Devices.Count);
        }

        public async Task Integration_Subscription_AddModules()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);

            ModuleModel model = ModuleModel.BuildModule(TestStatics.module_name, TestStatics.module_connectionString, TestStatics.module_moduleType);
            var updatedSubscription = await _subscriptionLogic.AddModuleAsync(model, TestStatics.device_id, subscription_User);

            subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            Assert.AreEqual(1, subscription.Devices.First().Modules.Count);
        }

        public async Task Integration_Subscription_AddSensors()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);

            SensorModel model = SensorModel.BuildSensor(TestStatics.sensor_name, TestStatics.sensor_displayName, TestStatics.sensor_type, TestStatics.module_name, TestStatics.sensor_pinConfig);
            var updatedSubscription = await _subscriptionLogic.AddSensorAsync(model, TestStatics.device_id, subscription_User);

            subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            Assert.AreEqual(1, subscription.Devices.First().Sensors.Count);
        }

        public async Task Integration_Subscription_AddRelays()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);

            RelayModel model = RelayModel.BuildModel(TestStatics.relay_number, TestStatics.relay_name, TestStatics.module_name, TestStatics.relay_pinConfig);
            var updatedSubscription = await _subscriptionLogic.AddRelayAsync(model, TestStatics.device_id, subscription_User);

            subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            Assert.AreEqual(1, subscription.Devices.First().Relays.Count);
        }

        public async Task Integration_Subscription_UpdateSubscription()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DateTime dateTimeBeforeUpdate = subscription.SubscriptionProperties.UpdatedTime.Value;

            string description = "This is an updated description";
            string name = "This is an updated name";

            SubscriptionPropertiesModel properties = subscription.SubscriptionProperties;
            properties.Description = description;
            properties.Name = name;

            var updatedSubscription = await _subscriptionLogic.UpdateSubscriptionPropertiesAsync(properties, subscription.SubscriptionProperties.User);

            Assert.AreEqual(description, updatedSubscription.SubscriptionProperties.Description);
            Assert.AreEqual(name, updatedSubscription.SubscriptionProperties.Name);
            Assert.IsTrue(updatedSubscription.SubscriptionProperties.UpdatedTime.Value > dateTimeBeforeUpdate);
            Assert.AreEqual(updatedSubscription.SubscriptionProperties.CreatedTime, subscription.SubscriptionProperties.CreatedTime);
        }

        public async Task Integration_Subscription_UpdateDevices()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DeviceModel device = subscription.Devices.Single(d => d.DeviceProperties.DeviceID == TestStatics.device_id);
            DateTime dateTimeBeforeUpdate = device.DeviceProperties.UpdatedTime.Value;

            DeviceModel updatedDevice = await _subscriptionLogic.UpdateDeviceAsync(device, subscription_User);

            Assert.IsNotNull(updatedDevice);
            Assert.AreEqual(device.DeviceProperties.CreatedTime, updatedDevice.DeviceProperties.CreatedTime);
            Assert.IsTrue(dateTimeBeforeUpdate < updatedDevice.DeviceProperties.UpdatedTime);

            // ensure the subscription still has one single device, not multiple
            subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            Assert.AreEqual(1, subscription.Devices.Count);
        }

        public async Task Integration_Subscription_UpdateModules()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DeviceModel device = subscription.Devices.Single(d => d.DeviceProperties.DeviceID == TestStatics.device_id);
            ModuleModel module = device.Modules.Single(m => m.Name == TestStatics.module_name);

            ModuleModel updatedModule = await _subscriptionLogic.UpdateModuleAsync(module, TestStatics.device_id, subscription_User);

            Assert.IsNotNull(updatedModule);

            // ensure the subscription still has one single device, not multiple
            device = await _subscriptionLogic.GetDeviceAsync(TestStatics.device_id, subscription_User, false);
            Assert.AreEqual(1, device.Modules.Count);

        }

        public async Task Integration_Subscription_UpdateSensors()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DeviceModel device = subscription.Devices.Single(d => d.DeviceProperties.DeviceID == TestStatics.device_id);
            SensorModel sensor = device.Sensors.Single(m => m.Name == TestStatics.sensor_name);

            SensorModel updatedSensor = await _subscriptionLogic.UpdateSensorAsync(sensor, TestStatics.device_id, subscription_User);

            Assert.IsNotNull(updatedSensor);

            // ensure the subscription still has one single device, not multiple
            device = await _subscriptionLogic.GetDeviceAsync(TestStatics.device_id, subscription_User, false);
            Assert.AreEqual(1, device.Sensors.Count);
        }

        public async Task Integration_Subscription_UpdateRelays()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DeviceModel device = subscription.Devices.Single(d => d.DeviceProperties.DeviceID == TestStatics.device_id);
            RelayModel relay = device.Relays.Single(m => m.Name == TestStatics.relay_name);

            RelayModel updatedRelay = await _subscriptionLogic.UpdateRelayAsync(relay, TestStatics.device_id, subscription_User);

            Assert.IsNotNull(updatedRelay);

            // ensure the subscription still has one single device, not multiple
            device = await _subscriptionLogic.GetDeviceAsync(TestStatics.device_id, subscription_User, false);
            Assert.AreEqual(1, device.Relays.Count);
        }

        public async Task Integration_Subscription_DeleteRelays()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DeviceModel device = subscription.Devices.Single(d => d.DeviceProperties.DeviceID == TestStatics.device_id);
            RelayModel relay = device.Relays.Single(m => m.Name == TestStatics.relay_name);

            await _subscriptionLogic.DeleteRelayAsync(relay, TestStatics.device_id, subscription_User);

            device = await _subscriptionLogic.GetDeviceAsync(TestStatics.device_id, subscription_User, false);
            Assert.AreEqual(0, device.Relays.Count);
        }

        public async Task Integration_Subscription_DeleteSensors()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DeviceModel device = subscription.Devices.Single(d => d.DeviceProperties.DeviceID == TestStatics.device_id);
            SensorModel sensor = device.Sensors.Single(m => m.Name == TestStatics.sensor_name);

            await _subscriptionLogic.DeleteSensorAsync(sensor, TestStatics.device_id, subscription_User);

            device = await _subscriptionLogic.GetDeviceAsync(TestStatics.device_id, subscription_User, false);
            Assert.AreEqual(0, device.Sensors.Count);
        }

        public async Task Integration_Subscription_DeleteModules()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DeviceModel device = subscription.Devices.Single(d => d.DeviceProperties.DeviceID == TestStatics.device_id);
            ModuleModel module = device.Modules.Single(m => m.Name == TestStatics.module_name);

            await _subscriptionLogic.DeleteModuleAsync(module, TestStatics.device_id, subscription_User);

            // ensure the subscription still has one single device, not multiple
            device = await _subscriptionLogic.GetDeviceAsync(TestStatics.device_id, subscription_User, false);
            Assert.AreEqual(0, device.Modules.Count);
        }

        public async Task Integration_Subscription_DeleteDevices()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            DeviceModel device = subscription.Devices.Single(d => d.DeviceProperties.DeviceID == TestStatics.device_id);

            await _subscriptionLogic.DeleteDeviceAsync(device, subscription_User);

            // ensure the subscription still has one single device, not multiple
            subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
            Assert.AreEqual(0, subscription.Devices.Count);
        }

        public async Task Integration_Subscription_DeleteSubscription()
        {
            SubscriptionModel subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);

            await _subscriptionLogic.DeleteSubscriptionAsync(subscription.SubscriptionProperties.SubscriptionID, subscription_User);

            // ensure the subscription still has one single device, not multiple
            subscription = await _subscriptionLogic.GetSubscriptionAsync(subscription_Id, subscription_User);
        }

        public void Dispose()
        {
            if (_autoMock != null)
            {
                _autoMock.Dispose();
                _autoMock = null;
            }
        }
    }
}