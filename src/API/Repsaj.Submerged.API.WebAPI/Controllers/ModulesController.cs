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
    [RoutePrefix("api/modules")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class ModulesController : ApiController
    {
        private readonly ISubscriptionLogic _subscriptionLogic;

        public ModulesController(ISubscriptionLogic subscriptionLogic)
        {
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("")]
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> Modules(string deviceId)
        {
            var modules = await _subscriptionLogic.GetModulesAsync(deviceId);
            return Ok(modules);
        }
    }
}
