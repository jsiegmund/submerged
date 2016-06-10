using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Models;
using System.Text;

namespace Repsaj.Submerged.API.Hubs
{
    public class LiveHub : Hub
    {
        IDeviceTelemetryLogic _deviceTelemetryLogic;
        public LiveHub(IDeviceTelemetryLogic deviceTelemetryLogic)
        {
            _deviceTelemetryLogic = deviceTelemetryLogic;
        }

        public void SendMessage(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.sendMessage(name, message);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public override Task OnConnected()
        {
            return (base.OnConnected());
        }

        public string GetServerTime()
        {
            return DateTime.UtcNow.ToString();
        }
    }
}