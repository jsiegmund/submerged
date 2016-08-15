using Newtonsoft.Json.Linq;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.DeviceSchema;
using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Common.Models.Commands;
using Repsaj.Submerged.Common.SubscriptionSchema;
using Repsaj.Submerged.Infrastructure.Exceptions;
using Repsaj.Submerged.Infrastructure.Models;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Repsaj.Submerged.Common.Helpers;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public class SubscriptionLogic : ISubscriptionLogic
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IConfigurationProvider _configProvider;
        private readonly IIotHubRepository _iotHubRepository;
        private readonly IDeviceRulesLogic _deviceRulesLogic;
        private readonly ISecurityKeyGenerator _securityKeyGenerator;
        private readonly IDeviceLogic _deviceLogic;
        private readonly INotificationHubRepository _notificationHubRepository;

        public SubscriptionLogic(ISubscriptionRepository subscriptionRepository, IConfigurationProvider configProvider,
            IIotHubRepository iotHubRepository, IDeviceRulesLogic deviceRulesLogic, ISecurityKeyGenerator securityKeyGenerator,
            IDeviceLogic deviceLogic, INotificationHubRepository notificationHubRepository)
        {
            _subscriptionRepository = subscriptionRepository;
            _configProvider = configProvider;
            _iotHubRepository = iotHubRepository;
            _deviceRulesLogic = deviceRulesLogic;
            _securityKeyGenerator = securityKeyGenerator;
            _deviceLogic = deviceLogic;
            _notificationHubRepository = notificationHubRepository;
        }

        public async Task<bool> ValidateDeviceOwnerAsync(string deviceId, string userId)
        {
            if (String.IsNullOrEmpty(deviceId))
                throw new ArgumentException("The deviceId cannot be null or empty.", "deviceId");

            if (String.IsNullOrEmpty(userId))
                throw new ArgumentException("The userId argument cannot be null or empty.", "userId");

            SubscriptionModel subscription = await _subscriptionRepository.GetSubscriptionByDeviceId(deviceId, "", true);

            if (subscription == null || 
                ! String.Equals(subscription.SubscriptionProperties.User, userId, StringComparison.InvariantCultureIgnoreCase))
                throw new SubscriptionValidationException(Strings.ValidationWrongUser);

            return true;
        }

        public async Task<SubscriptionModel> CreateSubscriptionAsync(string name, string description, string user, Guid? subscriptionId = null)
        {
            if (subscriptionId == null)
                subscriptionId = Guid.NewGuid();

            SubscriptionModel subscription = SubscriptionModel.BuildSubscription(subscriptionId.Value, name, description, user);

            // Validation logic throws an exception if it finds a validation error
            await ValidateSubscription(subscription);

            dynamic savedSubscription = await _subscriptionRepository.AddSubscriptionAsync(subscription);
            return (SubscriptionModel)savedSubscription;
        }


        public async Task<SubscriptionModel> GetSubscriptionAsync(Guid subscriptionId, string owner)
        {
            return await _subscriptionRepository.GetSubscriptionAsync(subscriptionId, owner);
        }

        public async Task<SubscriptionModel> GetSubscriptionAsync(string owner)
        {
            return await _subscriptionRepository.GetSubscriptionAsync(owner);
        }

        public async Task DeleteSubscriptionAsync(SubscriptionModel subscription)
        {
            await _subscriptionRepository.DeleteSubscriptionAsync(subscription);
        }

        public async Task DeleteSubscriptionAsync(Guid subscriptionId, string subscriptionUser)
        {
            SubscriptionModel subscription = await _subscriptionRepository.GetSubscriptionAsync(subscriptionId, subscriptionUser);

            if (subscription != null)
                await _subscriptionRepository.DeleteSubscriptionAsync(subscription);
        }

        public async Task<DeviceModel> GetDeviceAsync(string deviceId, string owner, bool skipValidation = false)
        {
            SubscriptionModel subscription = await _subscriptionRepository.GetSubscriptionByDeviceId(deviceId, owner, skipValidation);

            DeviceModel device = subscription.Devices.SingleOrDefault(d => d.DeviceProperties.DeviceID == deviceId);

            if (device == null)
                throw new DeviceNotRegisteredException(deviceId);

            return device;
        }

        public async Task<SubscriptionModel> UpdateSubscriptionPropertiesAsync(SubscriptionPropertiesModel properties, string owner)
        {
            // get the existing subscription and only update the subscription properties
            SubscriptionModel existing = await GetSubscriptionAsync(owner);

            if (existing == null)
                throw new SubscriptionUnknownException(Strings.ValidationSubscriptionUnknown);

            if (!String.Equals(properties.User, existing.SubscriptionProperties.User))
                throw new SubscriptionValidationException(Strings.ValidationUserChanged);
            if (properties.CreatedTime != existing.SubscriptionProperties.CreatedTime)
                throw new SubscriptionValidationException(Strings.ValidationCreatedChanged);

            existing.SubscriptionProperties = properties;

            return await UpdateSubscriptionAsync(existing, owner);
        }

        internal async Task<SubscriptionModel> UpdateSubscriptionAsync(SubscriptionModel subscription, string owner, bool skipValidation = false)
        {
            return await _subscriptionRepository.UpdateSubscriptionAsync(subscription, owner, skipValidation);
        }

        private bool ValidateSubscriptionId(SubscriptionModel subscription, List<string> validationErrors)
        {
            if (SubscriptionSchemaHelper.GetSubscriptionProperties(subscription) == null || SubscriptionSchemaHelper.GetSubscriptionID(subscription) == Guid.Empty)
            {
                validationErrors.Add(Strings.ValidationSubscriptionIdMissing);
                return false;
            }

            return true;
        }

        private async Task CheckIfSubscriptionExists(dynamic subscription, List<string> validationErrors)
        {
            // check if subscription exists
            if (await GetSubscriptionAsync(SubscriptionSchemaHelper.GetSubscriptionUser(subscription)) != null)
            {
                validationErrors.Add(Strings.ValidationSubscriptionExists);
            }
        }

        private async Task ValidateSubscription(SubscriptionModel subscription)
        {
            List<string> validationErrors = new List<string>();

            if (ValidateSubscriptionId(subscription, validationErrors))
            {
                await CheckIfSubscriptionExists(subscription, validationErrors);
            }

            if (validationErrors.Count > 0)
            {
                var validationException =
                    new SubscriptionValidationException(SubscriptionSchemaHelper.GetSubscriptionProperties(subscription) != null ? SubscriptionSchemaHelper.GetSubscriptionUser(subscription): null);

                foreach (string error in validationErrors)
                {
                    validationException.Errors.Add(error);
                }

                throw validationException;
            }
        }

        public async Task<IEnumerable<TankModel>> GetTanksAsync(string owner)
        {
            SubscriptionModel subscription = await GetSubscriptionAsync(owner);
            return subscription.Tanks;

        }

        public async Task<SubscriptionModel> AddTankAsync(TankModel tank, string owner)
        {
            SubscriptionModel subscription = await GetSubscriptionAsync(owner);

            if (subscription == null)
            {
                throw new SubscriptionUnknownException();
            }

            // check the tank isn't an existing one
            var existingTank = subscription.Tanks.SingleOrDefault(t => t.Name == tank.Name);
            if (existingTank != null)
            {
                throw new SubscriptionValidationException(owner);
            }

            // check there isn't another tank having the same name
            existingTank = subscription.Tanks.FirstOrDefault(t => t.Name == tank.Name);
            if (existingTank != null)
            {
                throw new SubscriptionValidationException(Strings.ValidationDuplicateTanks);
            }

            subscription.Tanks.Add(tank);
            return await UpdateSubscriptionAsync(subscription, owner);
        }

        public async Task<SubscriptionModel> UpdateTankAsync(TankModel updatedTank, string owner)
        {
            SubscriptionModel subscription = await GetSubscriptionAsync(owner);

            if (subscription == null)
            {
                throw new SubscriptionUnknownException();
            }

            // check the tank isn't an existing one
            var existingTank = subscription.Tanks.SingleOrDefault(t => t.Name == updatedTank.Name);
            if (existingTank == null)
            {
                throw new SubscriptionValidationException(Strings.ValidationTankNotFound);
            }

            subscription.Tanks.Remove(existingTank);
            subscription.Tanks.Add(updatedTank);

            return await UpdateSubscriptionAsync(subscription, owner);
        }

        public async Task<SubscriptionModel> DeleteTankAsync(TankModel tank, string owner)
        {
            SubscriptionModel subscription = await GetSubscriptionAsync(owner);

            if (subscription == null)
            {
                throw new SubscriptionUnknownException();
            }

            // check the tank isn't an existing one
            var existingTank = subscription.Tanks.SingleOrDefault(t => t.Name == tank.Name);
            if (existingTank == null)
            {
                throw new SubscriptionValidationException(Strings.ValidationTankNotFound);
            }

            subscription.Tanks.Remove(existingTank);
            return await UpdateSubscriptionAsync(subscription, owner);
        }

        public async Task<DeviceModel> AddDeviceAsync(DeviceModel device, string owner)
        {
            SubscriptionModel subscription = await GetSubscriptionAsync(owner);

            if (subscription == null)
            {
                throw new SubscriptionUnknownException();
            }

            // check the tank isn't an existing one
            var existingDevice = subscription.Devices.SingleOrDefault(t => t.DeviceProperties.DeviceID == device.DeviceProperties.DeviceID);
            if (existingDevice != null)
            {
                throw new SubscriptionValidationException(Strings.DeviceAlreadyRegisteredExceptionMessage);
            }

            // register a new device in the IoT hub instance 
            SecurityKeys generatedSecurityKeys = _securityKeyGenerator.CreateRandomKeys();
            await _iotHubRepository.AddDeviceAsync(device, generatedSecurityKeys);

            device.DeviceProperties.PrimaryKey = generatedSecurityKeys.PrimaryKey;
            device.DeviceProperties.SecondaryKey = generatedSecurityKeys.SecondaryKey;

            subscription.Devices.Add(device);

            await UpdateSubscriptionAsync(subscription, owner);

            return device;
        }

        public async Task SendDeviceConfigurationMessage(string deviceId)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, "", true);
            await SendDeviceConfigurationMessage(device);
        }

        private async Task SendDeviceConfigurationMessage(DeviceModel device)
        {
            DeviceCommandModel command = DeviceCommandModel.BuildDeviceCommand(DeviceCommandTypes.UPDATE_INFO);
            command.Parameters = device;

            // send the updated device configuration to the device via a cloud 2 device message
            await _iotHubRepository.SendCommand(device.DeviceProperties.DeviceID, command);
        }

        public async Task<DeviceModel> UpdateDeviceAsync(DeviceModel updatedDevice, string owner)
        {
            SubscriptionModel subscription = await _subscriptionRepository.GetSubscriptionByDeviceId(updatedDevice.DeviceProperties.DeviceID, owner);

            if (subscription == null)
            {
                throw new SubscriptionUnknownException();
            }

            // check the tank isn't an existing one
            var existingDevice = subscription.Devices.SingleOrDefault(t => t.DeviceProperties.DeviceID == updatedDevice.DeviceProperties.DeviceID);
            if (existingDevice == null)
            {
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);
            }

            updatedDevice.DeviceProperties.UpdatedTime = DateTime.UtcNow;

            subscription.Devices.Remove(existingDevice);
            subscription.Devices.Add(updatedDevice);

            // update the subscription 
            await UpdateSubscriptionAsync(subscription, owner);

            return updatedDevice;
        }


        /// <summary>
        /// Used by the event processor to update the initial data for the device
        /// without deleting the CommandHistory and the original created date
        /// This assumes the device controls and has full knowledge of its metadata except for:
        /// - CreatedTime
        /// - CommandHistory
        /// </summary>
        /// <param name="device">Device information to save to the backend Device Registry</param>
        /// <returns>Combined device that was saved to registry</returns>
        public async Task<dynamic> UpdateDeviceFromDeviceInfoPacketAsync(dynamic updatedDevice)
        {
            if (updatedDevice == null)
            {
                throw new ArgumentNullException("device");
            }

            string deviceId = DeviceSchemaHelper.GetDeviceID(updatedDevice);
            SubscriptionModel subscription = await _subscriptionRepository.GetSubscriptionByDeviceId(deviceId, "", true);

            // check the tank isn't an existing one
            var existingDevice = subscription.Devices.SingleOrDefault(t => t.DeviceProperties.DeviceID == deviceId);
            if (existingDevice == null)
            {
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);
            }

            foreach (var status in ((JArray)updatedDevice.ModuleStatus))
            {
                ModuleModel module = existingDevice.Modules.SingleOrDefault(m => m.Name == (string)status["Name"]);

                if (module != null)
                {
                    module.Status = (string)status["Status"];
                }
            }

            return await UpdateSubscriptionAsync(subscription, "", true);
        }

        public async Task DeleteDeviceAsync(DeviceModel updatedDevice, string owner)
        {
            SubscriptionModel subscription = await _subscriptionRepository.GetSubscriptionByDeviceId(updatedDevice.DeviceProperties.DeviceID, owner);

            if (subscription == null)
            {
                throw new SubscriptionUnknownException();
            }

            // check the device exists
            var existingDevice = subscription.Devices.SingleOrDefault(t => t.DeviceProperties.DeviceID == updatedDevice.DeviceProperties.DeviceID);
            if (existingDevice == null)
            {
                throw new SubscriptionValidationException(Strings.ValidationTankNotFound);
            }

            subscription.Devices.Remove(existingDevice);
            await UpdateSubscriptionAsync(subscription, owner);
        }
        
        public async Task<ModuleModel> AddModuleAsync(ModuleModel module, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            ModuleModel existingModule = device.Modules.FirstOrDefault(m => String.Equals(m.Name, module.Name));

            if (existingModule != null)
                throw new SubscriptionValidationException(Strings.ValidationModuleExists);

            string[] moduleTypes = typeof(ModuleTypes).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                      .Select(p => p.GetValue(null))
                                                      .Cast<string>()
                                                      .ToArray();

            // check that the given module type exists
            if (! moduleTypes.Contains(module.ModuleType))
                throw new SubscriptionValidationException(String.Format(Strings.ValidationInvalidModuleType, module.ModuleType));

            device.Modules.Add(module);

            await UpdateDeviceAsync(device, owner);
            await SendDeviceConfigurationMessage(device);

            return module;
        }

        public async Task<ModuleModel> UpdateModuleAsync(ModuleModel updatedModule, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            ModuleModel existingModule = device.Modules.FirstOrDefault(m => String.Equals(m.Name, updatedModule.Name));

            if (existingModule == null)
                throw new SubscriptionValidationException(String.Format(Strings.ValidationModuleUnknown, updatedModule.Name));

            device.Modules.Remove(existingModule);
            device.Modules.Add(updatedModule);

            await UpdateDeviceAsync(device, owner);
            await SendDeviceConfigurationMessage(device);

            return updatedModule;
        }

        public async Task DeleteModuleAsync(ModuleModel module, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            ModuleModel existingModule = device.Modules.FirstOrDefault(m => String.Equals(m.Name, module.Name));

            if (existingModule == null)
                throw new SubscriptionValidationException(String.Format(Strings.ValidationModuleUnknown, module.Name));

            device.Modules.Remove(existingModule);
            await UpdateDeviceAsync(device, owner);
        }

        public async Task<IEnumerable<ModuleModel>> GetModulesAsync(string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);
            return device.Modules.OrderBy(m => m.DisplayOrder);
        }

        public async Task UpdateLatestTelemetryData(string deviceId, dynamic deviceData, string owner, bool skipValidation = false)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner, skipValidation);

            device.LastTelemetryData = deviceData;

            await UpdateDeviceAsync(device, owner);
        }

        public async Task<dynamic> GetLatestTelemetryData(string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);
            return device.LastTelemetryData;
        }

        public async Task<IEnumerable<SensorModel>> GetSensorsAsync(string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);
            return device.Sensors.OrderBy(s => s.OrderNumber);
        }

        public async Task<SensorModel> AddSensorAsync(SensorModel sensor, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            if (device.Sensors.Exists(s => String.Equals(s.Name, sensor.Name)))
                throw new SubscriptionValidationException(Strings.ValidationSensorExists);
            if (! device.Modules.Exists(m => String.Equals(m.Name, sensor.Module)))
                throw new SubscriptionValidationException(String.Format(Strings.ValidationModuleUnknown, sensor.Module));

            var sensorTypes = ReflectionHelper.GetStringMemberValues(typeof(SensorTypes));
            if (!sensorTypes.Contains(sensor.SensorType))
                throw new SubscriptionValidationException(String.Format(Strings.ValidationInvalidSensorType, sensor.SensorType));

            device.Sensors.Add(sensor);

            await UpdateSensorRules(sensor, deviceId);
            await UpdateDeviceAsync(device, owner);

            return sensor;
        }

        private async Task DeleteSensorRules(SensorModel sensor, string deviceId)
        {
            // update the rules linked to this sensor
            List<DeviceRule> rules = await _deviceRulesLogic.GetDeviceRulesAsync(deviceId);

            // when a minrule for this sensor exists; delete it
            DeviceRule minRule = rules.SingleOrDefault(r => r.DataField == sensor.Name && r.Operator == DeviceRuleOperators.LessThen);
            if (minRule != null)
                await _deviceRulesLogic.DeleteDeviceRuleAsync(minRule.DeviceID, minRule.RuleId);

            // when a maxrule for this sensor exists; delete it
            DeviceRule maxRule = rules.SingleOrDefault(r => r.DataField == sensor.Name && r.Operator == DeviceRuleOperators.GreaterThen);
            if (maxRule != null)
                await _deviceRulesLogic.DeleteDeviceRuleAsync(maxRule.DeviceID, maxRule.RuleId);
        }

        private async Task UpdateSensorRules(SensorModel sensor, string deviceId)
        {
            // update the rules linked to this sensor
            List<DeviceRule> rules = await _deviceRulesLogic.GetDeviceRulesAsync(deviceId);

            // for all moisture && stockfloat sensor types, add a rule which triggers when the value reads 'true'
            if (sensor.SensorType == SensorTypes.MOISTURE || sensor.SensorType == SensorTypes.STOCKFLOAT)
            {
                DeviceRule sensorRule = rules?.SingleOrDefault(r => r.DataField == sensor.Name && r.Operator == DeviceRuleOperators.Equal);

                if (sensorRule == null)
                {
                    sensorRule = await _deviceRulesLogic.GetNewRuleAsync(deviceId);
                    sensorRule.Operator = DeviceRuleOperators.Equal;
                    sensorRule.DataField = sensor.Name;
                    sensorRule.DataType = sensor.SensorType;
                    sensorRule.RuleOutput = sensor.Name + "Alarm";
                    sensorRule.Threshold = 1;
                    sensorRule.EnabledState = true;

                    await _deviceRulesLogic.SaveDeviceRuleAsync(sensorRule);
                }
            }
            else
            {
                // find the min and max rules for this sensor, create new ones when they do not exist yet
                DeviceRule minRule = rules?.SingleOrDefault(r => r.DataField == sensor.Name && r.Operator == DeviceRuleOperators.LessThen);
                if (minRule == null)
                {
                    minRule = await _deviceRulesLogic.GetNewRuleAsync(deviceId);
                    minRule.Operator = DeviceRuleOperators.LessThen;
                    minRule.DataField = sensor.Name;
                    minRule.DataType = sensor.SensorType;
                    minRule.RuleOutput = sensor.Name + "MinAlarm";
                }

                minRule.Threshold = sensor.MinThreshold ?? 0;
                minRule.EnabledState = sensor.MinThresholdEnabled;

                await _deviceRulesLogic.SaveDeviceRuleAsync(minRule);

                DeviceRule maxRule = rules?.SingleOrDefault(r => r.DataField == sensor.Name && r.Operator == DeviceRuleOperators.GreaterThen);
                if (maxRule == null)
                {
                    maxRule = await _deviceRulesLogic.GetNewRuleAsync(deviceId);
                    maxRule.Operator = DeviceRuleOperators.GreaterThen;
                    maxRule.DataField = sensor.Name;
                    maxRule.DataType = sensor.SensorType;
                    maxRule.RuleOutput = sensor.Name + "MaxAlarm";
                }

                maxRule.Threshold = sensor.MaxThreshold ?? 0;
                maxRule.EnabledState = sensor.MaxThresholdEnabled;

                await _deviceRulesLogic.SaveDeviceRuleAsync(maxRule);

            }
        }

        public async Task<SensorModel> UpdateSensorAsync(SensorModel updatedSensor, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            SensorModel existingSensor = device.Sensors.FirstOrDefault(s => String.Equals(s.Name, updatedSensor.Name));

            if (existingSensor == null)
                throw new SubscriptionValidationException(String.Format(Strings.ValidationSensorUnknown, updatedSensor.Name));

            await UpdateSensorRules(updatedSensor, deviceId);

            device.Sensors.Remove(existingSensor);
            device.Sensors.Add(updatedSensor);

            await UpdateDeviceAsync(device, owner);

            return updatedSensor; 
        }

        public async Task DeleteSensorAsync(SensorModel sensor, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            SensorModel existingSensor = device.Sensors.FirstOrDefault(s => String.Equals(s.Name, sensor.Name));

            if (existingSensor == null)
                throw new SubscriptionValidationException(String.Format(Strings.ValidationSensorUnknown, sensor.Name));

            await DeleteSensorRules(sensor, deviceId);
            device.Sensors.Remove(existingSensor);

            await UpdateDeviceAsync(device, owner);
        }

        public async Task<IEnumerable<RelayModel>> GetRelaysAsync(string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);
            return device.Relays;
        }

        public async Task<RelayModel> AddRelayAsync(RelayModel relay, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            // verify the module selected exists
            if (!device.Modules.Exists(m => m.Name == relay.Module))
                throw new SubscriptionValidationException(Strings.ValidationModuleUnknown);

            // verify the relay doesn't exist already 
            if (device.Relays.Exists(r => r.Name == relay.Name))
                throw new SubscriptionValidationException(Strings.ValidationRelayExists);

            device.Relays.Add(relay);
            await UpdateDeviceAsync(device, owner);

            return relay;
        }
        public async Task<RelayModel> UpdateRelayAsync(RelayModel updatedRelay, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            RelayModel existingRelay = device.Relays.FirstOrDefault(s => s.Name == updatedRelay.Name);

            if (existingRelay == null)
                throw new SubscriptionValidationException(String.Format(Strings.ValidationRelayUnknown, updatedRelay.Name));

            device.Relays.Remove(existingRelay);
            device.Relays.Add(updatedRelay);

            await UpdateDeviceAsync(device, owner);
            return updatedRelay;
        }

        public async Task<RelayModel> UpdateRelayStateAsync(string name, bool state, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            RelayModel relay = device.Relays.FirstOrDefault(s => s.Name == name);

            if (relay == null)
                throw new SubscriptionValidationException(String.Format(Strings.ValidationRelayUnknown, relay.Name));

            if (relay.State != state)
            {
                relay.State = state;

                Dictionary<string, object> commandParams = new Dictionary<string, object>();
                commandParams.Add("Module", relay.Module);
                commandParams.Add("RelayNamer", name);
                commandParams.Add("RelayState", state);

                await _deviceLogic.SendCommandAsync(deviceId, DeviceCommandTypes.SWITCH_RELAY, commandParams);
            }

            await UpdateDeviceAsync(device, owner);

            return relay;
        }

        public async Task DeleteRelayAsync(RelayModel relay, string deviceId, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);

            RelayModel existingRelay = device.Relays.FirstOrDefault(s => s.Name == relay.Name);

            if (existingRelay == null)
                throw new SubscriptionValidationException(String.Format(Strings.ValidationRelayUnknown, existingRelay.Name));

            device.Relays.Remove(existingRelay);
            await UpdateDeviceAsync(device, owner);
        }

        public async Task<DeviceModel> SetMaintenance(string deviceId, bool inMaintenance, string owner)
        {
            DeviceModel device = await GetDeviceAsync(deviceId, owner);

            if (device == null)
                throw new SubscriptionValidationException(Strings.DeviceNotRegisteredExceptionMessage);
            if (device.DeviceProperties.IsInMaintenance && inMaintenance)
                throw new SubscriptionValidationException(Strings.DeviceAlreadyInMaintenanceMessage);
            if (!device.DeviceProperties.IsInMaintenance && !inMaintenance)
                throw new SubscriptionValidationException(Strings.DeviceNotInMaintenanceMessage);


            // ensure the maintenance bit is set for the rules output (notifications)
            await _deviceRulesLogic.OverrideDeviceRules(deviceId, inMaintenance);

            // fetch all relays that have been configured to toggle on maintenance and toggle them accordingly
            var relaysToToggle = device.Relays.Where(r => r.ToggleForMaintenance).Select(r => r.Name);
            foreach (string relayName in relaysToToggle)
            {
                var relay = device.Relays.Single(r => r.Name == relayName);

                // update our local model (otherwise the update device call later on will undo all changes)
                bool newState = !relay.State;
                relay.State = newState;

                // call the update relay to send the device command out
                await UpdateRelayStateAsync(relay.Name, newState, deviceId, owner);
            }

            // toggle the maintenance property on the model and save it
            device.DeviceProperties.IsInMaintenance = inMaintenance;
            return await UpdateDeviceAsync(device, owner);
        }

        public async Task UpdateNotificationInstallation(string installationId, string registrationId, string owner)
        {
            // fetch the subscription for the user
            var subscription = await this.GetSubscriptionAsync(owner);

            // create the list of tags that this device should subscribe to 
            List<string> tags = new List<string>();

            tags.Add(String.Format("subscription:{0}", subscription.Id));
            foreach (var device in subscription.Devices)
                tags.Add(String.Format("deviceid:{0}", device.DeviceProperties.DeviceID));

            await _notificationHubRepository.CreateOrUpdateInstallationAsync(installationId, registrationId, tags);
        }
    }
}
