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
        [HttpGet]
        public async Task<IHttpActionResult> Modules(string deviceId)
        {
            try
            {
                var modules = await _subscriptionLogic.GetModulesAsync(deviceId, AuthenticationHelper.UserId);
                return Ok(modules);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in Modules call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddModule(string deviceId, [FromBody]ModuleModel module)
        {
            try
            {
                await _subscriptionLogic.AddModuleAsync(module, deviceId, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in AddModule call: {0}", ex);
                return InternalServerError();
            }            
        }

        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateModule(string deviceId, [FromBody]ModuleModel module)
        {
            try
            {
                await _subscriptionLogic.UpdateModuleAsync(module, deviceId, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in UpdateModule call: {0}", ex);
                return InternalServerError();
            }            
        }

        [Route("")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteModule(string deviceId, [FromBody]ModuleModel module)
        {
            try
            {
                await _subscriptionLogic.DeleteModuleAsync(module, deviceId, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in DeleteModule call: {0}", ex);
                return InternalServerError();
            }            
        }
    }
}
