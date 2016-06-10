using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.BusinessLogic
{
    public interface IDeviceLogic
    {
        Task<DeviceWithKeys> AddDeviceAsync(dynamic device);
        Task SendCommandAsync(string deviceId, string commandName, dynamic parameters);
        Task<dynamic> UpdateDeviceFromDeviceInfoPacketAsync(dynamic device);
        Task<dynamic> GetDeviceAsync(string deviceId);
        Task<dynamic> UpdateDeviceAsync(dynamic device);

        void ApplyDevicePropertyValueModels(
            dynamic device,
            IEnumerable<DevicePropertyValueModel> devicePropertyValueModels);
        IEnumerable<DevicePropertyValueModel> ExtractDevicePropertyValuesModels(dynamic device);
    }
}
