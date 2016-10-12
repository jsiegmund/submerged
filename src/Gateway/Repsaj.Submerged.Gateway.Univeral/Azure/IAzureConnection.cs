using Newtonsoft.Json.Linq;
using Repsaj.Submerged.GatewayApp.Universal.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Universal.Azure
{
    delegate Task ICommandReceived(DeserializableCommand command);

    interface IAzureConnection
    {
        event ICommandReceived CommandReceived;
        event Action Connected;
        event Action Disconnected;

        Task Init();
        Task<string> SendDeviceToCloudMessageAsync(JObject eventData);
        Task<string> SendDeviceToCloudMessageAsync(string payload);
    }
}
