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

namespace Repsaj.Submerged.APITests
{
    [TestClass]
    public class SubscriptionTests : TestBase
    {
        [TestMethod]
        public async Task CanCreateSubscriptionAsync()
        {
            SubscriptionModel subscription = SubscriptionModel.BuildSubscription(Guid.NewGuid(), SubscriptionName, "", SubscriptionUser);
            await SubscriptionLogic.AddSubscriptionAsync(subscription);
        }

        [TestMethod]
        public async Task CanGetSubscriptionAsync()
        {
            SubscriptionModel subscription = await SubscriptionLogic.GetSubscriptionAsync(SubscriptionUser);
        }

        [TestMethod]
        public async Task CanUpdateSubscriptionAsync()
        {
            SubscriptionModel subscription = await SubscriptionLogic.GetSubscriptionAsync(SubscriptionUser);
            subscription.Tanks.Add(new TankModel(TankName, "This is a test description"));
            dynamic result = await SubscriptionLogic.UpdateSubscriptionAsync(subscription, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanAddTankToSubscription()
        {
            SubscriptionModel subscription = await SubscriptionLogic.GetSubscriptionAsync(SubscriptionUser);
            TankModel tank = TankModel.BuildTank(TankName, "This is a test description");
            await SubscriptionLogic.AddTankAsync(tank, SubscriptionUser);

        }

        [TestMethod]
        public async Task CanUpdateTank()
        {
            SubscriptionModel subscription = await SubscriptionLogic.GetSubscriptionAsync(SubscriptionUser);
            TankModel firstTank = subscription.Tanks.First();
            firstTank.Name = "Test";
            await SubscriptionLogic.UpdateTankAsync(firstTank, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanAddDeviceToSubscription()
        {
            SubscriptionModel subscription = await SubscriptionLogic.GetSubscriptionAsync(SubscriptionUser);
            DeviceModel device = DeviceModel.BuildDevice(DeviceId, false);
            await SubscriptionLogic.AddDeviceAsync(device, SubscriptionUser);

        }

        [TestMethod]
        public async Task CanUpdateDeviceAsync2()
        {
            SubscriptionModel subscription = await SubscriptionLogic.GetSubscriptionAsync(SubscriptionUser);
            DeviceModel firstDevice = subscription.Devices.First();
            await SubscriptionLogic.UpdateDeviceAsync(firstDevice, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanUpdateLatestTelemetryData()
        {
            string dataString = $"{{\"objectType\": \"Telemetry\",\"deviceId\": \"{DeviceId}\",\"temperature1\": 24.0,\"temperature2\": 21.5,\"pH\": 7.375}}";
            dynamic data = JsonConvert.DeserializeObject(dataString);
            await SubscriptionLogic.UpdateLatestTelemetryData(DeviceId, data, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanGetDeviceAsync2()
        {
            DeviceModel device = await SubscriptionLogic.GetDeviceAsync(DeviceId, SubscriptionUser);
            Assert.IsNotNull(device);
        }


        [TestMethod]
        public async Task CanAddModule()
        {
            //ModuleModel sensorModule = ModuleModel.BuildModule("Sensor Module", "", ModuleTypes.SENSORS);
            ModuleModel cabinetModule = ModuleModel.BuildModule("Cabinet Module", "", ModuleTypes.CABINET);

            //await SubscriptionLogic.AddModuleAsync(sensorModule, "Scubaline 240");
            await SubscriptionLogic.AddModuleAsync(cabinetModule, TankName, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanUpdateModule()
        {
            DeviceModel cabinetModule = await SubscriptionLogic.GetDeviceAsync(DeviceId, SubscriptionUser);
            ModuleModel module = cabinetModule.Modules.First();

            await SubscriptionLogic.UpdateModuleAsync(module, cabinetModule.DeviceProperties.DeviceID, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanGetModulesAsync()
        {
            IEnumerable<ModuleModel> deviceModules = await SubscriptionLogic.GetModulesAsync(DeviceId, SubscriptionUser);

            Assert.IsNotNull(deviceModules);
            Assert.IsTrue(deviceModules.Count() > 0);
        }

        [TestMethod]
        public async Task CanGetSensorsAsync()
        {
            IEnumerable<SensorModel> sensors = await SubscriptionLogic.GetSensorsAsync(DeviceId, SubscriptionUser);
            Assert.IsNotNull(sensors);
            Assert.IsTrue(sensors.Count() > 0);
        }

        [TestMethod]
        public async Task CanAddSensorAsync()
        {
            SensorModel sensor = SensorModel.BuildSensor("temperature1", "Temperature 1", SensorTypes.TEMPERATURE);
            await SubscriptionLogic.AddSensorAsync(sensor, DeviceId, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanUpdateSensorAsync()
        {
            IEnumerable<SensorModel> sensors = await SubscriptionLogic.GetSensorsAsync(DeviceId, SubscriptionUser);
            SensorModel sensor = sensors.First(r => r.Name == "temperature1");
            sensor.Description = "In tank temperature";
            sensor.MinThreshold = 22;
            sensor.MaxThreshold = 28;
            sensor.MinThresholdEnabled = true;
            sensor.MaxThresholdEnabled = true;
            await SubscriptionLogic.UpdateSensorAsync(sensor, DeviceId, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanGetRelaysAsync()
        {
            var relays = await SubscriptionLogic.GetRelaysAsync(DeviceId, SubscriptionUser);
            Assert.IsNotNull(relays);
            Assert.IsTrue(relays.Count() > 0);
        }

        [TestMethod]
        public async Task CanAddRelayAsync()
        {
            RelayModel relay1 = RelayModel.BuildModel(1, "Pump");
            await SubscriptionLogic.AddRelayAsync(relay1, DeviceId, SubscriptionUser);
            RelayModel relay2 = RelayModel.BuildModel(2, "Heating");
            await SubscriptionLogic.AddRelayAsync(relay2, DeviceId, SubscriptionUser);
            RelayModel relay3 = RelayModel.BuildModel(3, "Lights");
            await SubscriptionLogic.AddRelayAsync(relay3, DeviceId, SubscriptionUser);
        }

        [TestMethod]
        public async Task CanUpdateRelayAsync()
        {
            RelayModel relay = (await SubscriptionLogic.GetRelaysAsync(DeviceId, SubscriptionUser)).First();
            await SubscriptionLogic.UpdateRelayAsync(relay, DeviceId, SubscriptionUser);

        }

        [TestMethod]
        public async Task CanUpdateRelayStateAsync()
        {
            await SubscriptionLogic.UpdateRelayStateAsync(1, true, DeviceId, SubscriptionUser);
        }
    }
}
