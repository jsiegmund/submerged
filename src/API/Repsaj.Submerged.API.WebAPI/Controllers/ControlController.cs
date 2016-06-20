using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.API.Helpers;
using Repsaj.Submerged.Common.Models.Commands;
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

        [Route("maintenance/toggle")]
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> ToggleMaintenance(string deviceId, bool inMaintenance)
        {
            await _subscriptionLogic.SetMaintenance(deviceId, inMaintenance, AuthenticationHelper.UserId);
            return Ok();
        }


        [Route("setrelay")]
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> SetRelay(string deviceId, int relayNumber, bool state)
        {
            await _subscriptionLogic.UpdateRelayStateAsync(relayNumber, state, deviceId, AuthenticationHelper.UserId);

            return Ok();
        }
    }
}
