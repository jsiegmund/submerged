using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using Repsaj.Submerged.API.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Repsaj.Submerged.API.Controllers
{
    [RoutePrefix("api/test")]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class TestController : ApiControllerWithHub<LiveHub>
    {
        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult Get()
        {
            return Ok("");
        }
    }
}
