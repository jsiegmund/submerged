using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.API.Helpers;
using Repsaj.Submerged.Common.SubscriptionSchema;
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
    [RoutePrefix("api/relays")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class RelaysController : ApiController
    {
        private readonly ISubscriptionLogic _subscriptionLogic;

        public RelaysController(ISubscriptionLogic subscriptionLogic)
        {
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> Relays(string deviceId)
        {
            try
            {
                var sensorModels = await _subscriptionLogic.GetSensorsAsync(deviceId, AuthenticationHelper.UserId);
                return Ok(sensorModels);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in Relays call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddRelay(string deviceId, [FromBody]RelayModel relay)
        {
            try
            {
                await _subscriptionLogic.AddRelayAsync(relay, deviceId, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in AddRelay call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateRelay(string deviceId, [FromBody]RelayModel relay)
        {
            try
            {
                await _subscriptionLogic.UpdateRelayAsync(relay, deviceId, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in UpdateRelay call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteRelay(string deviceId, [FromBody]RelayModel relay)
        {
            try
            {
                await _subscriptionLogic.DeleteRelayAsync(relay, deviceId, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in DeleteRelay call: {0}", ex);
                return InternalServerError();
            }            
        }
    }
}