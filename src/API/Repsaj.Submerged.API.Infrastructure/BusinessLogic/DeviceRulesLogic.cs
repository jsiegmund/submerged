using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Infrastructure.Models;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public class DeviceRulesLogic : IDeviceRulesLogic
    {
        private readonly IDeviceRulesRepository _deviceRulesRepository;
        //private readonly IActionMappingLogic _actionMappingLogic;

        public DeviceRulesLogic(IDeviceRulesRepository deviceRulesRepository)
        {
            _deviceRulesRepository = deviceRulesRepository;
            //_actionMappingLogic = actionMappingLogic;
        }

        /// <summary>
        /// Retrieve the full list of Device Rules
        /// </summary>
        /// <returns></returns>
        public async Task<List<DeviceRule>> GetAllRulesAsync()
        {
            return await _deviceRulesRepository.GetAllRulesAsync();
        }

        /// <summary>
        /// Retrieve an existing rule for a device/ruleId pair. If a rule does not exist
        /// it will return null. This method is best used when you know the rule exists.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public async Task<List<DeviceRule>> GetDeviceRulesAsync(string deviceId)
        {
            return await _deviceRulesRepository.GetAllRulesForDeviceAsync(deviceId);
        }

        /// <summary>
        /// Retrieve an existing rule for a device/ruleId pair. If a rule does not exist
        /// it will return null. This method is best used when you know the rule exists.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public async Task<DeviceRule> GetDeviceRuleAsync(string deviceId, string ruleId)
        {
            return await _deviceRulesRepository.GetDeviceRuleAsync(deviceId, ruleId);
        }

        /// <summary>
        /// Generate a new rule with bare-bones configuration. This new rule can then be conigured and sent
        /// back through the SaveDeviceRuleAsync method to persist.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<DeviceRule> GetNewRuleAsync(string deviceId)
        {
            return await Task.Run(() =>
            {
                DeviceRule rule = DeviceRule.BuildRule(deviceId);
                return rule;
            });
        }       

        /// <summary>
        /// Save a rule to the data store. This method should be used for new rules as well as updating existing rules
        /// </summary>
        /// <param name="updatedRule"></param>
        /// <returns></returns>
        public async Task<TableStorageResponse<DeviceRule>> SaveDeviceRuleAsync(DeviceRule updatedRule)
        {
            //Enforce single instance of a rule for a data field for a given device
            List<DeviceRule> foundForDevice = await _deviceRulesRepository.GetAllRulesForDeviceAsync(updatedRule.DeviceID);
            foreach (DeviceRule rule in foundForDevice)
            {
                // prevent duplicates from being inserted having the same datafield and operator
                if (rule.DataField == updatedRule.DataField && rule.Operator == updatedRule.Operator && rule.RuleId != updatedRule.RuleId)
                {
                    var response = new TableStorageResponse<DeviceRule>();
                    response.Entity = rule;
                    response.Status = TableStorageResponseStatus.DuplicateInsert;

                    return response;
                }
            }

            return await _deviceRulesRepository.SaveDeviceRuleAsync(updatedRule);
        }

        public async Task<TableStorageResponse<DeviceRule>> DeleteDeviceRuleAsync(string deviceId, string ruleId)
        {
            DeviceRule found = await _deviceRulesRepository.GetDeviceRuleAsync(deviceId, ruleId);
            if (found == null)
            {
                var response = new TableStorageResponse<DeviceRule>();
                response.Entity = found;
                response.Status = TableStorageResponseStatus.NotFound;

                return response;
            }

            return await _deviceRulesRepository.DeleteDeviceRuleAsync(found);
        }

        public async Task<bool> RemoveAllRulesForDeviceAsync(string deviceId)
        {
            bool result = true;

            List<DeviceRule> deviceRules = await _deviceRulesRepository.GetAllRulesForDeviceAsync(deviceId);
            foreach (DeviceRule rule in deviceRules)
            {
                TableStorageResponse<DeviceRule> response = await _deviceRulesRepository.DeleteDeviceRuleAsync(rule);
                if (response.Status != TableStorageResponseStatus.Successful)
                {
                    //Do nothing, just report that it failed. The client can then take other steps if needed/desired
                    result = false;
                }
            }

            return result;
        }

        public async Task OverrideDeviceRules(string deviceId, bool enabled)
        {
            await _deviceRulesRepository.OverrideRulesForDevice(deviceId, enabled);
        }
    }
}
