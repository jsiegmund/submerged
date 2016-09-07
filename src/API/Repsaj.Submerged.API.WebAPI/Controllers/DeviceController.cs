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
    [AllowAnonymous]
    //[Authorize] TODO: switch back to authenticated calls
    [RoutePrefix("api/device")]
    [HostAuthentication("OAuth2Bearer")]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class DeviceController : ApiController
    {
        private readonly ISubscriptionLogic _subscriptionLogic;

        public DeviceController(ISubscriptionLogic subscriptionLogic)
        {
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("updatedeviceinfo")]
        [HttpPost]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> UpdateDeviceInfo([FromBody]dynamic deviceInfo)
        {
            try
            {
                await _subscriptionLogic.UpdateDeviceFromDeviceInfoPacketAsync(deviceInfo);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in UpdateDeviceInfo call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("updaterequest")]
        [HttpPost]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> UpdateRequest([FromBody]dynamic updateRequest)
        {
            try
            {
                if (updateRequest == null || updateRequest.DeviceID == null)
                    return BadRequest("The request didn't contain a valid DeviceID");

                await _subscriptionLogic.SendDeviceConfigurationMessage((string)updateRequest.DeviceID);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in UpdateRequest call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("~/api/devices")]
        [HttpPost]
        public async Task<IHttpActionResult> AddDevice([FromBody]DeviceModel device)
        {
            try
            {
                var newDevice = await _subscriptionLogic.AddDeviceAsync(device, AuthenticationHelper.UserId);
                return Ok(newDevice);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in AddDevice call: {0}", ex);
                return InternalServerError();
            }
        }

        [Route("~/api/devices")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteDevice([FromBody]DeviceModel device)
        {
            try
            {
                await _subscriptionLogic.DeleteDeviceAsync(device, AuthenticationHelper.UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in DeleteDevice call: {0}", ex);
                return InternalServerError();
            }
        }
    }
}
