using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.API.Helpers;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Repsaj.Submerged.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/subscription")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class SubscriptionController : ApiController
    {
        private readonly ISubscriptionLogic _subscriptionLogic;

        public SubscriptionController(ISubscriptionLogic subscriptionLogic)
        {
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> GetSubscription()
        {
            var deviceData = await _subscriptionLogic.GetSubscriptionAsync(AuthenticationHelper.UserId);
            return Ok(deviceData);
        }
    }
}