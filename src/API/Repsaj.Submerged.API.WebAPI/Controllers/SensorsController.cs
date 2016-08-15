using Microsoft.Azure.Mobile.Server.Config;
using Repsaj.Submerged.API.Helpers;
using Repsaj.Submerged.API.Models;
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
    [RoutePrefix("api/sensors")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class SensorsController : ApiController
    {
        private readonly ISubscriptionLogic _subscriptionLogic;

        public SensorsController(ISubscriptionLogic subscriptionLogic)
        {
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> Sensors(string deviceId)
        {
            var sensorModels = await _subscriptionLogic.GetSensorsAsync(deviceId, AuthenticationHelper.UserId);
            return Ok(sensorModels);
        }

        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddSensor(string deviceId, [FromBody]SensorModel sensor)
        {
            try
            {
                await _subscriptionLogic.AddSensorAsync(sensor, deviceId, AuthenticationHelper.UserId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok();
        }

        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateSensor(string deviceId, [FromBody]SensorModel sensor)
        {
            try
            {
                await _subscriptionLogic.UpdateSensorAsync(sensor, deviceId, AuthenticationHelper.UserId);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

            return Ok();
        }
        
        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> SaveSensors(string deviceId, [FromBody]SensorModel[] updatedSensors)
        {
            try
            {
                foreach(var updatedSensor in updatedSensors)
                {
                    await _subscriptionLogic.UpdateSensorAsync(updatedSensor, deviceId, AuthenticationHelper.UserId);
                }

                return Ok();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [Route("")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSensor(string deviceId, [FromBody]SensorModel sensor)
        {
            try
            {
                await _subscriptionLogic.DeleteSensorAsync(sensor, deviceId, AuthenticationHelper.UserId);
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok();
        }
    }
}
