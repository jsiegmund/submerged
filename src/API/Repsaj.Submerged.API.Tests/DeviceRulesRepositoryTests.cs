using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repsaj.Submerged.Infrastructure.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Repsaj.Submerged.APITests
{
    [TestClass]
    public class DeviceRulesRepositoryTests : TestBase
    {
        [TestMethod]
        public async Task CanSaveDeviceRuleAsync()
        {
            DeviceRule temperature1MinRule = await DeviceRulesLogic.GetNewRuleAsync(DeviceId);
            temperature1MinRule.DataField = "temperature1";
            temperature1MinRule.RuleOutput = "Temperature1MinAlarm";
            temperature1MinRule.Operator = "<";
            temperature1MinRule.Threshold = 22d;
            temperature1MinRule.DataType = "Temperature";
            await DeviceRulesLogic.SaveDeviceRuleAsync(temperature1MinRule);

            DeviceRule temperature1MaxRule = await DeviceRulesLogic.GetNewRuleAsync(DeviceId);
            temperature1MaxRule.DataField = "temperature1";
            temperature1MaxRule.RuleOutput = "Temperature1MaxAlarm";
            temperature1MaxRule.Operator = ">";
            temperature1MaxRule.Threshold = 28d;
            temperature1MaxRule.DataType = "Temperature";
            await DeviceRulesLogic.SaveDeviceRuleAsync(temperature1MaxRule);

            DeviceRule temperature2MinRule = await DeviceRulesLogic.GetNewRuleAsync(DeviceId);
            temperature2MinRule.DataField = "temperature2";
            temperature2MinRule.RuleOutput = "Temperature2MinAlarm";
            temperature2MinRule.Operator = "<";
            temperature2MinRule.Threshold = 15d;
            temperature2MinRule.DataType = "Temperature";
            await DeviceRulesLogic.SaveDeviceRuleAsync(temperature2MinRule);

            DeviceRule temperature2MaxRule = await DeviceRulesLogic.GetNewRuleAsync(DeviceId);
            temperature2MaxRule.DataField = "temperature2";
            temperature2MaxRule.RuleOutput = "Temperature2Alarm";
            temperature2MaxRule.Operator = ">";
            temperature2MaxRule.Threshold = 30d;
            temperature2MaxRule.DataType = "Temperature"; ;
            await DeviceRulesLogic.SaveDeviceRuleAsync(temperature2MaxRule);

            DeviceRule phMinRule = await DeviceRulesLogic.GetNewRuleAsync(DeviceId);
            phMinRule.DataField = "pH";
            phMinRule.RuleOutput = "pHAlarm";
            phMinRule.Operator = "<";
            phMinRule.Threshold = 6.0;
            phMinRule.EnabledState = true;
            phMinRule.DataType = "PH";
            await DeviceRulesLogic.SaveDeviceRuleAsync(phMinRule);

            DeviceRule phMaxRule = await DeviceRulesLogic.GetNewRuleAsync(DeviceId);
            phMaxRule.DataField = "pH";
            phMaxRule.RuleOutput = "pHAlarm";
            phMaxRule.Operator = ">";
            phMaxRule.Threshold = 7.5;
            phMaxRule.EnabledState = true;
            phMaxRule.DataType = "PH";
            await DeviceRulesLogic.SaveDeviceRuleAsync(phMaxRule);
        }

        [TestMethod]
        public async Task CanUpdateDeviceAsync()
        {
            var device = await DeviceLogic.GetDeviceAsync(DeviceId);
            await DeviceLogic.UpdateDeviceAsync(device);
        }

        [TestMethod]
        public async Task CanExtractDevicePropertyValuesModels()
        {
            var device = await DeviceLogic.GetDeviceAsync(DeviceId);
            DeviceLogic.ExtractDevicePropertyValuesModels(device);
        }


        [TestMethod]
        public async Task CanApplyDevicePropertyValueModels()
        {
            var device = await DeviceLogic.GetDeviceAsync(DeviceId);
            var propertyValueModel = DeviceLogic.ExtractDevicePropertyValuesModels(device);
            DeviceLogic.ApplyDevicePropertyValueModels(device, propertyValueModel);
        }

        [TestMethod]
        public async Task CanUpdateDeviceRuleAsync()
        {
            var rules = await DeviceRulesLogic.GetAllRulesAsync();
            var pHMinRule = rules.Single(r => r.DataField == "pH" && r.Operator == "<");
            pHMinRule.EnabledState = true;
            await DeviceRulesLogic.SaveDeviceRuleAsync(pHMinRule);

            var pHMaxRule = rules.Single(r => r.DataField == "pH" && r.Operator == ">");
            pHMaxRule.EnabledState = true;
            await DeviceRulesLogic.SaveDeviceRuleAsync(pHMaxRule);
        }

        [TestMethod]
        public async Task CanGetDeviceAsync()
        {
            dynamic device = await DeviceLogic.GetDeviceAsync(DeviceId);
        }

        [TestMethod]
        public async Task CanGetAllRulesAsync()
        {
            List<DeviceRule> rule = await DeviceRulesLogic.GetAllRulesAsync();
        }

        [TestMethod]
        public async Task CanGetDeviceRulesAsync()
        {
            List<DeviceRule> rule = await DeviceRulesLogic.GetDeviceRulesAsync(DeviceId);
        }

        [TestMethod]
        public async Task CanRemoveAllRulesForDeviceAsync()
        {
            await DeviceRulesLogic.RemoveAllRulesForDeviceAsync(DeviceId);
        }        
    }
}
