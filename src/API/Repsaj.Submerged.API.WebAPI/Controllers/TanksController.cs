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
    [RoutePrefix("api/tanks")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class TanksController : ApiController
    {
        private readonly ISubscriptionLogic _subscriptionLogic;

        public TanksController(ISubscriptionLogic subscriptionLogic)
        {
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> Tanks()
        {
            try
            {
                var tankModels = await _subscriptionLogic.GetTanksAsync(AuthenticationHelper.UserId);
                return Ok(tankModels);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in Tanks call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddTank([FromBody]TankModel tank)
        {
            try
            {
                await _subscriptionLogic.AddTankAsync(tank, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in AddTank call: {0}", ex);
                return InternalServerError();
            }            
        }

        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateTank([FromBody]TankModel tank)
        {
            try
            {
                await _subscriptionLogic.UpdateTankAsync(tank, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in UpdateTank call: {0}", ex);
                return InternalServerError();
            }            
        }


        [Route("")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteTank([FromBody]TankModel tank)
        {
            try
            {
                await _subscriptionLogic.DeleteTankAsync(tank, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in DeleteTank call: {0}", ex);
                return InternalServerError();
            }
        }
    }
}
