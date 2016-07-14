using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.API.Helpers;
using Repsaj.Submerged.Common.SubscriptionSchema;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Models;
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
    [RoutePrefix("api/tank/log")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class TankLogController : ApiController
    {
        private readonly ITankLogLogic _tankLogLogic;

        public TankLogController(ITankLogLogic tankLogLogic)
        {
            _tankLogLogic = tankLogLogic;
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> Post(TankLog log)
        {
            await _tankLogLogic.SaveTankLogAsync(log);
            return Ok();
        }

        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(Guid tankId)
        {
            var result = await _tankLogLogic.GetTankLogAsync(tankId);
            return Ok(result);
        }

        [Route("")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid tankId, Guid logId)
        {
            await _tankLogLogic.DeleteTankLogAsync(tankId, logId);
            return Ok();
        }
    }
}
