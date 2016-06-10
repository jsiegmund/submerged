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
        Task<SubscriptionModel> AddSubscriptionAsync(SubscriptionModel subscription);
        Task<SubscriptionModel> GetSubscriptionAsync(Guid subscriptionId);
        Task<SubscriptionModel> GetSubscriptionAsync(string subscriptionUser);
        Task<SubscriptionModel> UpdateSubscriptionAsync(SubscriptionModel device);
        Task<SubscriptionModel> AddTankAsync(TankModel tank, string subscriptionUser);
        Task<SubscriptionModel> UpdateTankAsync(TankModel tank, string subscriptionUser);
        Task<SubscriptionModel> DeleteTankAsync(TankModel tank, string subscriptionUser);
        Task<SubscriptionModel> AddDeviceAsync(DeviceModel device, string subscriptionUser);
        Task<DeviceModel> GetDeviceAsync(string deviceId);
        Task<DeviceModel> UpdateDeviceAsync(DeviceModel device);
        Task<dynamic> UpdateDeviceFromDeviceInfoPacketAsync(dynamic device);
        Task DeleteDeviceAsync(DeviceModel device);

        Task<IEnumerable<ModuleModel>> GetModulesAsync(string deviceId);
        Task<ModuleModel> AddModuleAsync(ModuleModel module, string deviceId);
        Task<ModuleModel> UpdateModuleAsync(ModuleModel module, string deviceId);
        Task DeleteModuleAsync(ModuleModel module, string deviceId);

        Task UpdateLatestTelemetryData(string deviceId, dynamic deviceData);
        Task<dynamic> GetLatestTelemetryData(string deviceId);

        Task<IEnumerable<SensorModel>> GetSensorsAsync(string deviceId);
        Task<SensorModel> AddSensorAsync(SensorModel sensor, string deviceId);
        Task<SensorModel> UpdateSensorAsync(SensorModel updatedSensor, string deviceId);
        Task DeleteSensorAsync(SensorModel sensor, string deviceId);

        Task<IEnumerable<RelayModel>> GetRelaysAsync(string deviceId);
        Task<RelayModel> AddRelayAsync(RelayModel relay, string deviceId);
        Task<RelayModel> UpdateRelayAsync(RelayModel updatedRelay, string deviceId);
        Task<RelayModel> UpdateRelayStateAsync(int relayNumber, bool state, string deviceId);
        Task DeleteRelayAsync(RelayModel relay, string deviceId);
    }
}
