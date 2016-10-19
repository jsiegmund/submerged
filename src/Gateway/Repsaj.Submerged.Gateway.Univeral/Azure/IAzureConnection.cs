using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Commands;
using Repsaj.Submerged.GatewayApp.Universal.Models;
using Repsaj.Submerged.GatewayApp.Universal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Azure
{
    public delegate Task ICommandReceived(DeserializableCommand command);

    public interface IAzureConnection
    {
        event ICommandReceived CommandReceived;
        event Action Connected;
        event Action Disconnected;

        AzureConnectionStatus Status { get; }

        Task<bool> SendDeviceToCloudMessageAsync(JObject eventData);
        Task<bool> SendDeviceToCloudMessageAsync(string payload);
        Task Init(ConnectionInformationModel connectionInfo);
    }
}
