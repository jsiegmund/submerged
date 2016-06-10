using Repsaj.Submerged.API.Models;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Repsaj.Submerged.API.Helpers
{
    //public class RulesHelper
    //{
    //    public async static Task<IEnumerable<SensorModel>> GetRules(IDeviceRulesLogic deviceRulesLogic, string deviceId)
    //    {
    //        IEnumerable<DeviceRule> deviceRules = await deviceRulesLogic.GetDeviceRulesAsync(deviceId);
    //        deviceRules = deviceRules.OrderBy(r => r.Threshold);

    //        List<SensorModel> result = new List<SensorModel>();

    //        foreach (var deviceRule in deviceRules)
    //        {
    //            SensorModel ruleModel = new SensorModel()
    //            {
    //                Name = deviceRule.DataField,
    //                Enabled = deviceRule.EnabledState,
    //                Threshold = deviceRule.Threshold,
    //            };

    //            switch (deviceRule.DataType.ToLower())
    //            {
    //                case "temperature":
    //                    ruleModel.MinimumValue = 10;
    //                    ruleModel.MaximumValue = 40;
    //                    ruleModel.Step = 0.5;
    //                    break;
    //                case "ph":
    //                    ruleModel.MinimumValue = 5.0;
    //                    ruleModel.MaximumValue = 9.0;
    //                    ruleModel.Step = 0.1;
    //                    break;
    //            }

    //            result.Add(ruleModel);
    //        }

    //        return result;
    //    }

    //    public async static Task SaveRules(IDeviceRulesLogic deviceRulesLogic, IEnumerable<SensorModel> apiRules, string deviceId)
    //    {
    //        var deviceRules = await deviceRulesLogic.GetAllRulesAsync();

    //        List<Task> saveActions = new List<Task>();

    //        foreach (string dataField in apiRules.Select(r => r.Name).Distinct())
    //        {
    //            SensorModel[] apiRulesSensor = apiRules.Where(r => r.Name == dataField).ToArray();
    //            DeviceRule[] deviceRulesSensor = deviceRules.Where(r => r.DataField == dataField).ToArray();

    //            await SaveRules(deviceRulesLogic, apiRulesSensor, deviceRulesSensor);
    //        }
    //    }

    //    private async static Task SaveRules(IDeviceRulesLogic deviceRulesLogic, IEnumerable<SensorModel> apiRules, IEnumerable<DeviceRule> deviceRules)
    //    {
    //        apiRules = apiRules.OrderBy(r => r.Threshold);

    //        var firstRule = apiRules.First();
    //        var secondRule = apiRules.Skip(1).First();

    //        var low = deviceRules.Single(r => r.Operator == "<");
    //        low.Threshold = firstRule.Threshold;
    //        low.EnabledState = firstRule.Enabled;
    //        await deviceRulesLogic.SaveDeviceRuleAsync(low);

    //        var high = deviceRules.Single(r => r.Operator == ">");
    //        high.Threshold = secondRule.Threshold;
    //        high.EnabledState = secondRule.Enabled;
    //        await deviceRulesLogic.SaveDeviceRuleAsync(high);
    //    }
    //}
}