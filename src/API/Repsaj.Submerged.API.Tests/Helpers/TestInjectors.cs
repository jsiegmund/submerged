using Autofac.Extras.Moq;
using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Common.SubscriptionSchema;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Models;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests.Helpers
{
    public static class TestInjectors
    {
        public static void InjectMockedSecurityKey(AutoMock autoMock)
        {
            autoMock.Mock<ISecurityKeyGenerator>()
                .Setup(x => x.CreateRandomKeys())
                .Returns(new SecurityKeys("key1", "key2"));
        }

        public static void InjectMockedSubscription(AutoMock autoMock, bool addDevice = false)
        {
            DeviceModel device = null;

            // if a device should be added; generate one 
            if (addDevice)
                device = DeviceModel.BuildDevice(TestConfigHelper.DeviceId, true);

            InjectMockedSubscription(autoMock, device);
        }

        public static void InjectMockedSubscription(AutoMock autoMock, TankModel tank)
        {
            var subscription = SubscriptionModel.BuildSubscription(Guid.NewGuid(), TestConfigHelper.SubscriptionName, "Test Subscription", TestConfigHelper.SubscriptionUser);
            subscription.Tanks.Add(tank);

            autoMock.Mock<ISubscriptionRepository>()
                .Setup(x => x.GetSubscriptionAsync(TestConfigHelper.SubscriptionUser))
                .Returns(() => Task.FromResult<SubscriptionModel>(subscription));
        }

        public static void InjectMockedSubscription(AutoMock autoMock, DeviceModel device)
        {
            var subscription = SubscriptionModel.BuildSubscription(Guid.NewGuid(), TestConfigHelper.SubscriptionName, "Test Subscription", TestConfigHelper.SubscriptionUser);

            // add a preconfigured device to the subscription when required
            if (device != null)
                subscription.Devices.Add(device);

            autoMock.Mock<ISubscriptionRepository>()
                .Setup(x => x.GetSubscriptionAsync(TestConfigHelper.SubscriptionUser))
                .Returns(() => Task.FromResult<SubscriptionModel>(subscription));
            autoMock.Mock<ISubscriptionRepository>()
                .Setup(x => x.GetSubscriptionByDeviceId(TestConfigHelper.DeviceId, TestConfigHelper.SubscriptionUser, false))
                .Returns(() => Task.FromResult<SubscriptionModel>(subscription));
        }

        internal static void InjectDeviceRules(AutoMock autoMock, DeviceRule rule)
        {
            if (rule == null)
            {
                rule = DeviceRule.BuildRule(TestConfigHelper.DeviceId);
            }

            autoMock.Mock<IDeviceRulesLogic>()
                    .Setup(x => x.GetNewRuleAsync(TestConfigHelper.DeviceId))
                    .Returns(() => Task.FromResult(rule));

            autoMock.Mock<IDeviceRulesLogic>()
                    .Setup(x => x.GetDeviceRulesAsync(TestConfigHelper.DeviceId))
                    .Returns(() => Task.FromResult(new List<DeviceRule>(new DeviceRule[] { rule })));

            autoMock.Mock<IDeviceRulesRepository>()
                    .Setup(x => x.GetAllRulesForDeviceAsync(TestConfigHelper.DeviceId))
                    .Returns(() => Task.FromResult(new List<DeviceRule>(new DeviceRule[] { rule })));
        }
    }
}
