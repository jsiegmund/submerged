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
            var tankModels = await _subscriptionLogic.GetTanksAsync(AuthenticationHelper.UserId);
            return Ok(tankModels);
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddTank([FromBody]TankModel tank)
        {
            try
            {
                await _subscriptionLogic.AddTankAsync(tank, AuthenticationHelper.UserId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok();
        }

        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateTank([FromBody]TankModel tank)
        {
            try
            {
                await _subscriptionLogic.UpdateTankAsync(tank, AuthenticationHelper.UserId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok();
        }


        [Route("")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteTank([FromBody]TankModel tank)
        {
            try
            {
                await _subscriptionLogic.DeleteTankAsync(tank, AuthenticationHelper.UserId);
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok();
        }
    }
}
