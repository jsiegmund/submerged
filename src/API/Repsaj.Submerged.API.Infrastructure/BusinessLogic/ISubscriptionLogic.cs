using Repsaj.Submerged.Common.SubscriptionSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public interface ISubscriptionLogic
    {
        Task<SubscriptionModel> CreateSubscriptionAsync(string name, string description, string user, Guid? subscripionId = null);
        Task<SubscriptionModel> GetSubscriptionAsync(Guid subscriptionId, string owner);
        Task<SubscriptionModel> GetSubscriptionAsync(string owner);
        Task<SubscriptionModel> UpdateSubscriptionPropertiesAsync(SubscriptionPropertiesModel properties, string owner);
        Task DeleteSubscriptionAsync(SubscriptionModel _subscription);
        Task DeleteSubscriptionAsync(Guid subscription_Id, string subscriptionUser);


        Task<IEnumerable<TankModel>> GetTanksAsync(string owner);
        Task<SubscriptionModel> AddTankAsync(TankModel tank, string owner);
        Task<SubscriptionModel> UpdateTankAsync(TankModel tank, string owner);
        Task<SubscriptionModel> DeleteTankAsync(TankModel tank, string owner);


        Task<DeviceModel> GetDeviceAsync(string deviceId, string owner, bool skipValidation = false);
        Task<DeviceModel> AddDeviceAsync(DeviceModel device, string owner);
        Task<DeviceModel> UpdateDeviceAsync(DeviceModel device, string owner);
        Task DeleteDeviceAsync(DeviceModel device, string owner);


        Task<IEnumerable<ModuleModel>> GetModulesAsync(string deviceId, string owner);
        Task<ModuleModel> AddModuleAsync(ModuleModel module, string deviceId, string owner);
        Task<ModuleModel> UpdateModuleAsync(ModuleModel module, string deviceId, string owner);
        Task DeleteModuleAsync(ModuleModel module, string deviceId, string owner);


        //Task<dynamic> GetLatestTelemetryData(string deviceId, string owner);
        //Task UpdateLatestTelemetryData(string deviceId, dynamic deviceData, string owner, bool skipValidation = false);


        Task<IEnumerable<SensorModel>> GetSensorsAsync(string deviceId, string owner);
        Task<SensorModel> AddSensorAsync(SensorModel sensor, string deviceId, string owner);
        Task<SensorModel> UpdateSensorAsync(SensorModel updatedSensor, string deviceId, string owner);
        Task DeleteSensorAsync(SensorModel sensor, string deviceId, string owner);


        Task<IEnumerable<RelayModel>> GetRelaysAsync(string deviceId, string owner);
        Task<RelayModel> AddRelayAsync(RelayModel relay, string deviceId, string owner);
        Task<RelayModel> UpdateRelayAsync(RelayModel updatedRelay, string deviceId, string owner);
        Task<RelayModel> UpdateRelayStateAsync(string name, bool state, string deviceId, string owner);
        Task DeleteRelayAsync(RelayModel relay, string deviceId, string owner);


        Task<dynamic> UpdateDeviceFromDeviceInfoPacketAsync(dynamic device);
        Task<bool> ValidateDeviceOwnerAsync(string deviceId, string owner);
        Task SendDeviceConfigurationMessage(string deviceId);
        Task<DeviceModel> SetMaintenance(string deviceId, bool inMaintenance, string owner);

        Task UpdateNotificationInstallation(string installationId, string registrationId, string owner);
    }
}
