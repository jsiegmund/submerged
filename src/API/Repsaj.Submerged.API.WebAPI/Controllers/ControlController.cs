using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.API.Helpers;
using Repsaj.Submerged.Common.Models.Commands;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            try
            {
                var relays = await _subscriptionLogic.GetRelaysAsync(deviceId, AuthenticationHelper.UserId);
                return Ok(relays);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in Relays call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("setrelay")]
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> UpdateRelay(string deviceId, string name, bool state)
        {
            try
            {
                await _subscriptionLogic.UpdateRelayStateAsync(name, state, deviceId, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in UpdateRelay call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("maintenance/toggle")]
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> ToggleMaintenance(string deviceId, bool inMaintenance)
        {
            try
            {
                await _subscriptionLogic.SetMaintenance(deviceId, inMaintenance, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in ToggleMaintenance call: {0}", ex);
                return InternalServerError();
            }
        }

    }
}
