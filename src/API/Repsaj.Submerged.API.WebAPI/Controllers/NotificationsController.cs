using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.API.Helpers;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Models;
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
    [RoutePrefix("api/notifications")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class NotificationsController : ApiController
    {
        ISubscriptionLogic _subscriptionLogic;

        public NotificationsController(ISubscriptionLogic subscriptionLogic)
        {
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateNotificationInstallation(string registrationId)
        {
            try
            {
                string installationId = this.Request.Headers.GetValues("X-ZUMO-INSTALLATION-ID").First();
                await _subscriptionLogic.UpdateNotificationInstallation(installationId, registrationId, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in UpdateNotificationInstallation call: {0}", ex);
                return InternalServerError();
            }            
        }
    }
}
