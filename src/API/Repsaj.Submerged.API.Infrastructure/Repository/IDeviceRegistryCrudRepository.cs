using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public interface IDeviceRegistryCrudRepository
    {
        /// <summary>
        /// Adds a device asynchronously.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        Task<dynamic> AddDeviceAsync(dynamic device);

        /// <summary>
        /// Gets a device asynchronously.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        Task<dynamic> GetDeviceAsync(string deviceId);

        /// <summary>
        /// Updates a device asynchronously.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        Task<dynamic> UpdateDeviceAsync(dynamic device);

    }
}