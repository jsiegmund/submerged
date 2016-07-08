using Autofac.Extras.Moq;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Infrastructure.BusinessLogic;
using Repsaj.Submerged.Infrastructure.Models;
using Repsaj.Submerged.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.API.Tests.Integration
{
    public class TankLogIntegrationContext : IDisposable
    {
        private AutoMock _autoMock;

        private Guid log_tankId = new Guid("{00000000-0000-0000-0000-000000000000}");
        private Guid log_Id = new Guid("{00000000-0000-0000-0000-000000000000}");
        private string log_title = "Integration test log";
        private string log_description = "Integration test log item. This item was inserted during integration testing. It should be ignored when it hasn't been properly cleaned up.";
        private string log_logType = "Integration test";

        private ITankLogLogic _tankLogLogic;
        private TankLog _tankLog;

        public void Initialize()
        {
            _autoMock = AutoMock.GetLoose();

            _autoMock.Provide<IConfigurationProvider, ConfigurationProvider>();

            // inject the actual repositories
            _autoMock.Provide<ITankLogRepository, TankLogRepository>();

            // Logic
            _autoMock.Provide<ITankLogLogic, TankLogLogic>();

            _tankLogLogic = _autoMock.Create<ITankLogLogic>();

            _tankLog = new TankLog()
            {
                LogId = log_Id,
                TankId = log_tankId,
                Title = log_title,
                Description = log_description,
                LogType = log_logType
            };
        }

        public async Task Integration_TankLog_CreateLog()
        {
            await _tankLogLogic.SaveTankLogAsync(_tankLog);
        }

        public async Task<TankLog> Integration_TankLog_GetLog()
        {
            return await _tankLogLogic.GetTankLogAsync(log_tankId, log_Id);
        }

        public async Task<IEnumerable<TankLog>> Integration_TankLog_GetTankLog()
        {
            return await _tankLogLogic.GetTankLogAsync(log_tankId);
        }

        public async Task Integration_TankLog_DeleteLog()
        {
            await _tankLogLogic.DeleteTankLogAsync(log_tankId, log_Id);
        }

        public void Dispose()
        {
            if (_autoMock != null)
            {
                _autoMock.Dispose();
                _autoMock = null;
            }
        }
    }
}
