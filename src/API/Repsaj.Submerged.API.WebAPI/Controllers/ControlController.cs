using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Repsaj.Submerged.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/control")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class ControlController : ApiController
    {
        private readonly IDeviceLogic _deviceLogic;
        private readonly ISubscriptionLogic _subscriptionLogic;

        public ControlController(IDeviceLogic deviceLogic, ISubscriptionLogic subscriptionLogic)
        {
            _deviceLogic = deviceLogic;
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("relays")]
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> Relays(string deviceId)
        {
            var relays = await _subscriptionLogic.GetRelaysAsync(deviceId);
            return Ok(relays);
        }


        [Route("setrelay")]
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> SetRelay(string deviceId, int relayNumber, bool state)
        {
            Dictionary<string, object> commandParams = new Dictionary<string, object>();
            commandParams.Add("RelayNumber", relayNumber);
            commandParams.Add("RelayState", state);

            // send the command to the device
            await _deviceLogic.SendCommandAsync(deviceId, "SwitchRelay", commandParams);

            // store the new state of the relay after the command has been sent
            await _subscriptionLogic.UpdateRelayStateAsync(relayNumber, state, deviceId);

            return Ok();
        }
    }
}
