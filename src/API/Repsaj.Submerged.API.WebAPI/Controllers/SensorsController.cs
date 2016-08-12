﻿using Microsoft.Azure.Mobile.Server.Config;
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
        [HttpPost]
        [HttpGet]
        public async Task<IHttpActionResult> Sensors(string deviceId)
        {
            var sensorModels = await _subscriptionLogic.GetSensorsAsync(deviceId, AuthenticationHelper.UserId);
            return Ok(sensorModels);
        }


        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> SaveSensor(string deviceId, [FromBody]SensorModel sensor)
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
        
        [Route("save")]
        [HttpPost]
        [HttpGet]
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
    }
}
