using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repsaj.Submerged.Infrastructure.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Repsaj.Submerged.API.Tests.Helpers;
using Autofac.Extras.Moq;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Repository;
using Repsaj.Submerged.Common.Models;

namespace Repsaj.Submerged.APITests
{
    [TestClass]
    public class DeviceRulesRepositoryTests
    {
        [TestMethod]
        public async Task Save_NewDeviceRule_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var newRule = DeviceRule.BuildRule(TestConfigHelper.DeviceId);

                autoMock.Mock<IDeviceRulesLogic>()
                        .Setup(x => x.GetNewRuleAsync(TestConfigHelper.DeviceId))
                        .Returns(() => Task.FromResult(newRule));

                autoMock.Mock<IDeviceRulesRepository>()
                        .Setup(x => x.GetAllRulesForDeviceAsync(TestConfigHelper.DeviceId))
                        .Returns(() => Task.FromResult(new List<DeviceRule>()));

                var deviceRulesLogic = autoMock.Create<DeviceRulesLogic>();

                DeviceRule temperature1MinRule = await deviceRulesLogic.GetNewRuleAsync(TestConfigHelper.DeviceId);
                temperature1MinRule.DataField = "temperature1";
                temperature1MinRule.RuleOutput = "Temperature1MinAlarm";
                temperature1MinRule.Operator = "<";
                temperature1MinRule.Threshold = 22d;
                temperature1MinRule.DataType = "Temperature";
                await deviceRulesLogic.SaveDeviceRuleAsync(temperature1MinRule);
            }
        }

        [TestMethod]
        public async Task Save_ExistingDeviceRule_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                DeviceRule newRule = DeviceRule.BuildRule(TestConfigHelper.DeviceId);

                autoMock.Mock<IDeviceRulesLogic>()
                        .Setup(x => x.GetNewRuleAsync(TestConfigHelper.DeviceId))
                        .Returns(() => Task.FromResult(newRule));

                var existingRule = DeviceRule.BuildRule(TestConfigHelper.DeviceId);
                existingRule.DataField = "temperature1";
                existingRule.Operator = "<";

                autoMock.Mock<IDeviceRulesRepository>()
                        .Setup(x => x.GetAllRulesForDeviceAsync(TestConfigHelper.DeviceId))
                        .Returns(() => Task.FromResult(new List<DeviceRule>(new DeviceRule[] { existingRule })));

                var deviceRulesLogic = autoMock.Create<DeviceRulesLogic>();

                DeviceRule temperature1MinRule = await deviceRulesLogic.GetNewRuleAsync(TestConfigHelper.DeviceId);
                temperature1MinRule.DataField = "temperature1";
                temperature1MinRule.RuleOutput = "Temperature1MinAlarm";
                temperature1MinRule.Operator = "<";
                temperature1MinRule.Threshold = 22d;
                temperature1MinRule.DataType = "Temperature";
                await deviceRulesLogic.SaveDeviceRuleAsync(temperature1MinRule);
            }
        }

        [TestMethod]
        public async Task CanUpdateDeviceRuleAsync()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                DeviceRule newRule = DeviceRule.BuildRule(TestConfigHelper.DeviceId);
                TestInjectors.InjectMockedSubscription(autoMock, true);
                TestInjectors.InjectDeviceRules(autoMock, newRule);

                var deviceRulesLogic = autoMock.Create<DeviceRulesLogic>();

                newRule.EnabledState = false;
                await deviceRulesLogic.SaveDeviceRuleAsync(newRule);
            }
        }

        [TestMethod]
        public async Task Get_AllDeviceRules_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var deviceRulesLogic = autoMock.Create<DeviceRulesLogic>();
                List<DeviceRule> rule = await deviceRulesLogic.GetAllRulesAsync();
            }
        }

        [TestMethod]
        public async Task Get_DeviceRulesForDevice_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                var deviceRulesLogic = autoMock.Create<DeviceRulesLogic>();
                List<DeviceRule> rule = await deviceRulesLogic.GetDeviceRulesAsync(TestConfigHelper.DeviceId);
            }
        }

        [TestMethod]
        public async Task Remove_RulesForDevice_Success()
        {
            using (var autoMock = AutoMock.GetLoose())
            {
                DeviceRule newRule = DeviceRule.BuildRule(TestConfigHelper.DeviceId);
                TestInjectors.InjectMockedSubscription(autoMock, true);
                TestInjectors.InjectDeviceRules(autoMock, newRule);

                autoMock.Mock<IDeviceRulesRepository>()
                        .Setup(x => x.DeleteDeviceRuleAsync(newRule))
                        .Returns(() => Task.FromResult(new TableStorageResponse<DeviceRule>() { Status = TableStorageResponseStatus.Successful }));

                var deviceRulesLogic = autoMock.Create<DeviceRulesLogic>();
                await deviceRulesLogic.RemoveAllRulesForDeviceAsync(TestConfigHelper.DeviceId);
            }
        }
    }
}
