using Microsoft.Azure.Mobile.Server.Config;
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
    [RoutePrefix("api/data")]
    [MobileAppController]
    [System.Web.Mvc.OutputCache(CacheProfile = "NoCacheProfile")]
    public class DataController : ApiController
    {
        private readonly IDeviceTelemetryLogic _deviceTelemetryLogic;
        private readonly ISubscriptionLogic _subscriptionLogic;

        public DataController(IDeviceTelemetryLogic deviceTelemetryLogic, ISubscriptionLogic subscriptionLogic)
        {
            _deviceTelemetryLogic = deviceTelemetryLogic;
            _subscriptionLogic = subscriptionLogic;
        }

        [Route("latest")]
        [HttpGet]
        public async Task<IHttpActionResult> Latest(string deviceId)
        {
            try
            {

                var deviceData = await _deviceTelemetryLogic.LoadLatestDeviceTelemetryAsync(deviceId);
                return Ok(deviceData);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Route("threehours")]
        [HttpGet]
        public async Task<IHttpActionResult> LastThreeHours(string deviceId, DateTime date, int offset = 0)
        {
            try
            {
                var deviceData = await _deviceTelemetryLogic.LoadDeviceTelemetryReportDataLastThreeHoursAsync(deviceId, date, offset);
                return Ok(deviceData);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Route("hour")]
        [HttpGet]
        public async Task<IHttpActionResult> Hour(string deviceId, DateTime date, int offset = 0)
        {
            try
            {
                var deviceData = await _deviceTelemetryLogic.LoadDeviceTelemetryReportDataPerHourAsync(deviceId, date, offset);
                return Ok(deviceData);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Route("day")]
        [HttpGet]
        public async Task<IHttpActionResult> Day(string deviceId, DateTime date, int offset = 0)
        {
            try
            {
                var deviceData = await _deviceTelemetryLogic.LoadDeviceTelemetryReportDataPerDayAsync(deviceId, date, offset);
                return Ok(deviceData);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Route("week")]
        [HttpGet]
        public async Task<IHttpActionResult> Week(string deviceId, DateTime date, int offset = 0)
        {
            try
            {
                var deviceData = await _deviceTelemetryLogic.LoadDeviceTelemetryReportDataPerWeekAsync(deviceId, date, offset);
                return Ok(deviceData);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Route("month")]
        [HttpGet]
        public async Task<IHttpActionResult> Month(string deviceId, DateTime date, int offset = 0)
        {
            try
            {
                var deviceData = await _deviceTelemetryLogic.LoadDeviceTelemetryReportDataPerMonthAsync(deviceId, date, offset);
                return Ok(deviceData);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }
    }
}
