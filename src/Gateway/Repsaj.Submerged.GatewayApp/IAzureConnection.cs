using Newtonsoft.Json.Linq;
using RemoteArduino.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.GatewayApp
{
    delegate Task ICommandReceived(DeserializableCommand command);

    interface IAzureConnection
    {
        event ICommandReceived CommandReceived;

        Task<string> SendDeviceToCloudMessagesAsync(JObject eventData);
    }
}
