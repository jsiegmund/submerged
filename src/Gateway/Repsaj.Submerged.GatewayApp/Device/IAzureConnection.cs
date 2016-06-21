using Newtonsoft.Json.Linq;
using RemoteArduino.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp.Device
{
    delegate Task ICommandReceived(DeserializableCommand command);

    interface IAzureConnection
    {
        event ICommandReceived CommandReceived;
        event Action Connected;
        event Action Disconnected;

        Task<string> SendDeviceToCloudMessagesAsync(JObject eventData);
    }
}
