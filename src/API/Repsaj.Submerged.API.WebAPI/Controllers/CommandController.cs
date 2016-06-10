using Microsoft.Azure.Mobile.Server.Config;
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
    [RoutePrefix("api/command")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class CommandController : ApiController
    {
        [Route("")]
        public IHttpActionResult Post()
        {
            // This method serves as a checkpoint to see whether the mobile app can call the 
            // api. It just returns a 200 ok to validate the authenticated connection.
            return Ok("Ok");
        }

        [Route("")]
        public IHttpActionResult Get()
        {
            // This method serves as a checkpoint to see whether the mobile app can call the 
            // api. It just returns a 200 ok to validate the authenticated connection.
            return Ok("Ok");
        }
    }
}
