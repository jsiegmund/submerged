using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.API.Helpers;
using Repsaj.Submerged.Common.SubscriptionSchema;
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
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> Relays(string deviceId)
        {
            var sensorModels = await _subscriptionLogic.GetSensorsAsync(deviceId, AuthenticationHelper.UserId);
            return Ok(sensorModels);
        }


        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> SaveRelay(string deviceId, [FromBody]RelayModel relay)
        {
            try
            {
                await _subscriptionLogic.UpdateRelayAsync(relay, deviceId, AuthenticationHelper.UserId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok();
        }
    }
}