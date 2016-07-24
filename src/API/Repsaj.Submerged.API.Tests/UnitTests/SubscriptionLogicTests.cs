using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repsaj.Submerged.Common.SubscriptionSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.Common.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Configuration;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Autofac.Extras.Moq;
using Repsaj.Submerged.API.Tests.Helpers;
using Repsaj.Submerged.Infrastructure.Repository;
using Moq;
using Repsaj.Submerged.Infrastructure.Exceptions;
using Repsaj.Submerged.Infrastructure.Models;

namespace Repsaj.Submerged.API.Tests.UnitTests
{
    [TestClass]
    public class SubscriptionLogicTests
    {
        [TestMethod]
        public async Task Create_Subscription_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                var result = await subscriptionLogic.CreateSubscriptionAsync(TestConfigHelper.SubscriptionName, TestStatics.subscription_description, TestConfigHelper.SubscriptionUser);

                // the created subscription will not be returned because the mocked data layer will return null
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Create_Subscription_DuplicateException()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock);
                // should throw an error because an existing subscription is being returned
                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.CreateSubscriptionAsync(TestConfigHelper.SubscriptionName, "Test description", TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Get_Subscription_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                SubscriptionModel subscription = await subscriptionLogic.GetSubscriptionAsync(TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Update_Subscription_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();

                SubscriptionModel subscription = await subscriptionLogic.GetSubscriptionAsync(TestConfigHelper.SubscriptionUser);
                subscription.SubscriptionProperties.SubscriptionEnabledState = false;
                dynamic result = await subscriptionLogic.UpdateSubscriptionAsync(subscription, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Update_Subscription_FailureUserChanged()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();

                SubscriptionModel subscription = await subscriptionLogic.GetSubscriptionAsync(TestConfigHelper.SubscriptionUser);

                subscription.SubscriptionProperties.CreatedTime = DateTime.Now;

                dynamic result = await subscriptionLogic.UpdateSubscriptionPropertiesAsync(subscription.SubscriptionProperties, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Update_SubscriptionProperties_FailureCreatedChanged()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();

                SubscriptionModel subscription = await subscriptionLogic.GetSubscriptionAsync(TestConfigHelper.SubscriptionUser);

                // deliberately change User which should not be altered
                subscription.SubscriptionProperties.User = "";

                dynamic result = await subscriptionLogic.UpdateSubscriptionPropertiesAsync(subscription.SubscriptionProperties, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Add_Tank_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();

                TankModel tank = TankModel.BuildTank(TestConfigHelper.TankName, "This is a test description");
                await subscriptionLogic.AddTankAsync(tank, TestConfigHelper.SubscriptionUser);

            }
        }

        [TestMethod]
        public async Task Update_Tank_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                TankModel tank = new TankModel("Test Tank", "Put your description here.");
                TestInjectors.InjectMockedSubscription(autoMock, tank);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.UpdateTankAsync(tank, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Add_Device_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock);
                TestInjectors.InjectMockedSecurityKey(autoMock);

                DeviceModel model = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, false);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                var device = await subscriptionLogic.AddDeviceAsync(model, TestConfigHelper.SubscriptionUser);

                Assert.AreEqual(TestConfigHelper.DeviceId, device.DeviceProperties.DeviceID);
                Assert.IsNotNull(device.DeviceProperties.CreatedTime);
                Assert.IsNotNull(device.DeviceProperties.UpdatedTime);
                Assert.AreEqual(false, device.DeviceProperties.IsInMaintenance);
                Assert.AreEqual(false, device.DeviceProperties.IsSimulatedDevice);
                Assert.IsNotNull(device.DeviceProperties.PrimaryKey);
                Assert.IsNotNull(device.DeviceProperties.SecondaryKey);
                Assert.IsNull(device.DeviceProperties.DisplayOrder);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Add_Device_Duplicate()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, true);
                TestInjectors.InjectMockedSecurityKey(autoMock);

                DeviceModel device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, false);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.AddDeviceAsync(device, TestConfigHelper.SubscriptionUser);

            }
        }

        [TestMethod]
        public async Task Update_Device_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, true);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                SubscriptionModel subscription = await subscriptionLogic.GetSubscriptionAsync(TestConfigHelper.SubscriptionUser);
                DeviceModel firstDevice = subscription.Devices.First();
                await subscriptionLogic.UpdateDeviceAsync(firstDevice, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Update_Device_NotFound()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, false);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                await subscriptionLogic.UpdateDeviceAsync(device, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Update_LatestTelemetryData_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, true);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                string dataString = $"{{\"objectType\": \"Telemetry\",\"deviceId\": \"{TestConfigHelper.DeviceId}\",\"temperature1\": 24.0,\"temperature2\": 21.5,\"pH\": 7.375}}";
                dynamic data = JsonConvert.DeserializeObject(dataString);
                await subscriptionLogic.UpdateLatestTelemetryData(TestConfigHelper.DeviceId, data, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Get_Device_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, true);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                DeviceModel device = await subscriptionLogic.GetDeviceAsync(TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
                Assert.IsNotNull(device);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DeviceNotRegisteredException))]
        public async Task Get_Device_NotFound()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, false);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                DeviceModel device = await subscriptionLogic.GetDeviceAsync(TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
                Assert.IsNotNull(device);
            }
        }

        [TestMethod]
        public async Task Add_Module_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, true);
                ModuleModel cabinetModule = ModuleModel.BuildModule("Cabinet Module", "", ModuleTypes.CABINET);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.AddModuleAsync(cabinetModule, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Add_Module_WrongType()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, true);
                ModuleModel cabinetModule = ModuleModel.BuildModule("Cabinet Module", "", "WRONG");

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.AddModuleAsync(cabinetModule, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Add_Module_Duplicate()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, true);

                ModuleModel cabinetModule1 = ModuleModel.BuildModule("Cabinet Module", "Test module 1", ModuleTypes.CABINET);
                ModuleModel cabinetModule2 = ModuleModel.BuildModule("Cabinet Module", "Test module 2", ModuleTypes.CABINET);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.AddModuleAsync(cabinetModule1, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
                await subscriptionLogic.AddModuleAsync(cabinetModule2, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Update_Module_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                ModuleModel module = new ModuleModel()
                {
                    ConnectionString = "TestConnectionString",
                    Name = "Test Module"
                };
                device.Modules.Add(module);
                TestInjectors.InjectMockedSubscription(autoMock, device);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.UpdateModuleAsync(module, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Update_Module_NotFound()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                ModuleModel module = new ModuleModel()
                {
                    ConnectionString = "TestConnectionString",
                    Name = "Test Module"
                };
                TestInjectors.InjectMockedSubscription(autoMock, device);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.UpdateModuleAsync(module, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Get_ModulesForDevice_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                ModuleModel module = new ModuleModel()
                {
                    ConnectionString = "TestConnectionString",
                    Name = "Test Module"
                };
                device.Modules.Add(module);
                TestInjectors.InjectMockedSubscription(autoMock, device);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                IEnumerable<ModuleModel> deviceModules = await subscriptionLogic.GetModulesAsync(TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);

                Assert.IsNotNull(deviceModules);
                Assert.AreEqual(1, deviceModules.Count());
            }
        }

        [TestMethod]
        public async Task Get_SensorsForDevice_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);

                TestInjectors.InjectMockedSubscription(autoMock, device);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                IEnumerable<SensorModel> sensors = await subscriptionLogic.GetSensorsAsync(TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
                Assert.IsNotNull(sensors);
            }
        }

        [TestMethod]
        public async Task Add_Sensor_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                ModuleModel module = ModuleModel.BuildModule(TestStatics.module_name, TestStatics.module_connectionString, TestStatics.module_moduleType);
                device.Modules.Add(module);
                TestInjectors.InjectMockedSubscription(autoMock, device);
                TestInjectors.InjectDeviceRules(autoMock, null);

                SensorModel sensor = SensorModel.BuildSensor(TestStatics.sensor_name, TestStatics.sensor_displayName, TestStatics.sensor_type, module.Name, TestStatics.sensor_pinConfig);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                SensorModel newModel = await subscriptionLogic.AddSensorAsync(sensor, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);

                Assert.AreEqual(TestStatics.sensor_name, newModel.Name);
                Assert.AreEqual(TestStatics.sensor_displayName, newModel.DisplayName);
                Assert.AreEqual(TestStatics.sensor_type, newModel.SensorType);
                Assert.AreEqual(TestStatics.module_name, newModel.Module);
                Assert.AreEqual(TestStatics.sensor_pinConfig, sensor.PinConfig);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Add_Sensor_WrongType()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                ModuleModel module = ModuleModel.BuildModule("Test Module", "", ModuleTypes.SENSORS);
                device.Modules.Add(module);
                TestInjectors.InjectMockedSubscription(autoMock, device);
                TestInjectors.InjectDeviceRules(autoMock, null);

                SensorModel sensor = SensorModel.BuildSensor(TestStatics.sensor_name, TestStatics.sensor_description, "WRONG", module.Name, TestStatics.sensor_pinConfig);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.AddSensorAsync(sensor, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SubscriptionValidationException))]
        public async Task Add_Sensor_Duplicate()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);

                var module = TestInjectors.InjectMockedModule(device);
                TestInjectors.InjectMockedSubscription(autoMock, device);
                TestInjectors.InjectDeviceRules(autoMock, null);

                SensorModel sensor1 = SensorModel.BuildSensor(TestStatics.sensor_name, TestStatics.sensor_description, TestStatics.sensor_type, module.Name, TestStatics.sensor_pinConfig);
                SensorModel sensor2 = SensorModel.BuildSensor(TestStatics.sensor_name, TestStatics.sensor_description, TestStatics.sensor_type, module.Name, TestStatics.sensor_pinConfig);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.AddSensorAsync(sensor1, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
                await subscriptionLogic.AddSensorAsync(sensor2, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Update_Sensor_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);

                ModuleModel module = ModuleModel.BuildModule(TestStatics.module_name, TestStatics.module_description, TestStatics.module_moduleType);
                device.Modules.Add(module);
                SensorModel sensor = SensorModel.BuildSensor(TestStatics.sensor_name, TestStatics.sensor_description, TestStatics.sensor_type, module.Name, TestStatics.sensor_pinConfig);
                device.Sensors.Add(sensor);

                TestInjectors.InjectMockedSubscription(autoMock, device);
                TestInjectors.InjectDeviceRules(autoMock, null);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.UpdateSensorAsync(sensor, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Get_RelaysForDevice_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                TestInjectors.InjectMockedSubscription(autoMock, true);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                var relays = await subscriptionLogic.GetRelaysAsync(TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
                Assert.IsNotNull(relays);
            }
        }
        [TestMethod]
        public async Task Add_Relay_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                ModuleModel module = ModuleModel.BuildModule("Test Module", "", ModuleTypes.SENSORS);
                device.Modules.Add(module);
                TestInjectors.InjectMockedSubscription(autoMock, device);

                RelayModel relay1 = RelayModel.BuildModel(1, "Pump", module.Name);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                var relay = await subscriptionLogic.AddRelayAsync(relay1, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
                Assert.IsNotNull(relay);
            }
        }

        [TestMethod]
        public async Task Update_Relay_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                RelayModel relay = new RelayModel()
                {
                    OrderNumber = 1,
                    RelayNumber = 1,
                    State = true,
                    Name = "Test Module"
                };
                device.Relays.Add(relay);
                TestInjectors.InjectMockedSubscription(autoMock, device);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.UpdateRelayAsync(relay, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);

            }
        }

        [TestMethod]
        public async Task Update_RelayState_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                RelayModel relay = new RelayModel()
                {
                    OrderNumber = 1,
                    RelayNumber = 1,
                    State = true,
                    Name = "Test Module"
                };
                device.Relays.Add(relay);
                TestInjectors.InjectMockedSubscription(autoMock, device);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                await subscriptionLogic.UpdateRelayStateAsync(1, true, TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser);
            }
        }

        [TestMethod]
        public async Task Update_MaintenanceMode_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);
                RelayModel relay = new RelayModel()
                {
                    OrderNumber = 1,
                    RelayNumber = 1,
                    State = true,
                    Name = "Test Module",
                    ToggleForMaintenance = true
                };
                device.Relays.Add(relay);
                TestInjectors.InjectMockedSubscription(autoMock, device);

                Assert.AreEqual(false, device.DeviceProperties.IsInMaintenance);

                var subscriptionLogic = autoMock.Create<SubscriptionLogic>();
                device = await subscriptionLogic.SetMaintenance(TestConfigHelper.DeviceId, true, TestConfigHelper.SubscriptionUser);

                Assert.AreEqual(true, device.DeviceProperties.IsInMaintenance);
                Assert.AreEqual(false, device.Relays.First().State);
            }
        }
    }
}
    