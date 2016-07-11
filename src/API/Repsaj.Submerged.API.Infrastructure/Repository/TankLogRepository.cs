using Microsoft.WindowsAzure.Storage.Table;
using Repsaj.Submerged.Common.Configurations;
using Repsaj.Submerged.Common.Helpers;
using Repsaj.Submerged.Common.Models;
using Repsaj.Submerged.Common.SubscriptionSchema;
using Repsaj.Submerged.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repsaj.Submerged.Infrastructure.Repository
{
    public class TankLogRepository : ITankLogRepository
    {
        private readonly IConfigurationProvider _configurationProvider;

        private readonly string _storageAccountConnectionString;
        private readonly string _tankLogNormalizedTableName;
        public TankLogRepository(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;

            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _tankLogNormalizedTableName = configurationProvider.GetConfigurationSettingValue("TankLogTableName");
        }

        public async Task<List<TankLog>> GetTankLogAsync(Guid tankId)
        {
            List<TankLog> result = new List<TankLog>();

            IEnumerable<TankLogTableEntity> queryResults = await GetLogsFromTableForTank(tankId);
            foreach (TankLogTableEntity log in queryResults)
            {
                var logLine = BuildLogFromTableEntity(log);
                result.Add(logLine);
            }

            return result;
        }

        private async Task<IEnumerable<TankLogTableEntity>> GetLogsFromTableForTank(Guid tankId)
        {
            var logLinesTable = await AzureTableStorageHelper.GetTableAsync(_storageAccountConnectionString, _tankLogNormalizedTableName);
            TableQuery<TankLogTableEntity> query = new TableQuery<TankLogTableEntity>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tankId.ToString()));

            return await Task.Run(() =>
                logLinesTable.ExecuteQuery(query)
            );
        }

        public async Task<TableStorageResponse<TankLog>> SaveTankLogAsync(TankLog logLine)
        {
            TankLogTableEntity incomingEntity = BuildTableEntityFromRule(logLine);

            TableStorageResponse<TankLog> result =
                await AzureTableStorageHelper.DoTableInsertOrReplaceAsync<TankLog, TankLogTableEntity>(incomingEntity, BuildLogFromTableEntity,
                    _storageAccountConnectionString, _tankLogNormalizedTableName);

            return result;
        }

        private TankLog BuildLogFromTableEntity(TankLogTableEntity tableEntity)
        {
            if (tableEntity == null)
            {
                return null;
            }

            var result = new TankLog(tableEntity.TankId)
            {
                Description = tableEntity.Description,
                ETag = tableEntity.ETag,
                LogId = new Guid(tableEntity.LogId),
                LogType = tableEntity.LogType,
                TimeCreated = tableEntity.TimeCreated,
                Title = tableEntity.Title
            };

            return result;
        }

        private TankLogTableEntity BuildTableEntityFromRule(TankLog incomingLog)
        {
            TankLogTableEntity tableEntity =
                new TankLogTableEntity(incomingLog.TankId, incomingLog.LogId.ToString())
                {
                    Description = incomingLog.Description,
                    LogType = incomingLog.LogType,
                    TimeCreated = incomingLog.TimeCreated,
                    Title = incomingLog.Title
                };

            if (!string.IsNullOrEmpty(incomingLog.ETag))
            {
                tableEntity.ETag = incomingLog.ETag;
            }

            return tableEntity;
        }

        /// <summary>
        /// Retrieve a single log line from AzureTableStorage. A distincts rule
        /// is defined by the combination key tankId / logId
        /// </summary>
        /// <param name="tankId"></param>
        /// <param name="logId"></param>
        /// <returns></returns>
        public async Task<TankLog> GetTankLogAsync(Guid tankId, Guid logId)
        {
            var logLinesTable = await AzureTableStorageHelper.GetTableAsync(_storageAccountConnectionString, _tankLogNormalizedTableName);
            TableOperation query = TableOperation.Retrieve<TankLogTableEntity>(tankId.ToString(), logId.ToString());

            TableResult response = await Task.Run(() =>
                logLinesTable.Execute(query)
            );

            TankLog result = BuildLogFromTableEntity((TankLogTableEntity)response.Result);
            return result;
        }

        public async Task<TableStorageResponse<TankLog>> DeleteTankLogAsync(TankLog logLine)
        {
            TankLogTableEntity incomingEntity = BuildTableEntityFromRule(logLine);

            TableStorageResponse<TankLog> result =
                await AzureTableStorageHelper.DoDeleteAsync<TankLog, TankLogTableEntity>(incomingEntity, BuildLogFromTableEntity,
                    _storageAccountConnectionString, _tankLogNormalizedTableName);

            return result;
        }
    }
}
