using System;
using System.Collections.Generic;
using System.Linq;

using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.Queries;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Settings;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class MonitoringServiceImpl : IMonitoringServiceImpl
    {
        public MonitoringServiceImpl(
            ILocalStorage localStorage,
            ILocalStorageTableRegistry localStorageTableRegistry,
            ISqlDatabase sqlDatabase,
            ILocalStorageUpdater localStorageUpdater,
            IMonitoringServiceSettings monitoringServiceSettings,
            CounterService counterService)
        {
            this.localStorage = localStorage;
            this.sqlDatabase = sqlDatabase;
            taskMetaInfoTableName = localStorageTableRegistry.GetTableName(typeof(MonitoringTaskMetadata));
            this.localStorageUpdater = localStorageUpdater;
            this.monitoringServiceSettings = monitoringServiceSettings;
            this.counterService = counterService;
        }

        public void ActualizeDatabaseScheme()
        {
            localStorage.ActualizeDatabaseScheme();
            counterService.Init();
        }

        public void DropLocalStorage()
        {
            localStorage.DropDatabase();
            localStorageUpdater.ClearProcessedEvents();
            counterService.Restart(null);
        }

        public int GetCount(MonitoringGetCountQuery getCountQuery)
        {
            if(monitoringServiceSettings.ActualizeOnQuery)
                localStorageUpdater.Update();
            return localStorage.GetCount<MonitoringTaskMetadata>(getCountQuery.Criterion);
        }

        public TaskCount GetProcessingTaskCount()
        {
            return counterService.GetCount();
        }

        public void RecalculateInProcess()
        {
            localStorageUpdater.RecalculateInProcess();
        }

        public MonitoringTaskMetadata[] Search(MonitoringSearchQuery searchQuery)
        {
            if(monitoringServiceSettings.ActualizeOnQuery)
                localStorageUpdater.Update();
            return localStorage.SearchWithoutScopeIdIdSort<MonitoringTaskMetadata>(searchQuery.Criterion, searchQuery.RangeFrom, searchQuery.Count, searchQuery.SortRules);
        }

        public object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery)
        {
            if(monitoringServiceSettings.ActualizeOnQuery)
                localStorageUpdater.Update();
            var sqlSelectQuery = new SqlSelectQuery
                {
                    Criterion = getDistinctValuesQuery.Criterion,
                    NeedColumns = new[] {getDistinctValuesQuery.ColumnPath.Path},
                    Distinct = true,
                    TableName = taskMetaInfoTableName
                };
            var debagRes = sqlDatabase.GetRows(sqlSelectQuery).Select(x => x[0]).ToArray();
            return debagRes;
        }

        public MonitoringTaskMetadata[] GetTaskWithAllDescendants(string taskId)
        {
            if(monitoringServiceSettings.ActualizeOnQuery)
                localStorageUpdater.Update();
            var task = localStorage.Get<MonitoringTaskMetadata>(taskId, taskId);
            if(task == null)
                return null;
            var list = new List<MonitoringTaskMetadata>();
            GetTaskWithAllDescendants(task, list);
            return list.ToArray();
        }

        public void RestartProcessgingTaskCounter(DateTime? fromTime)
        {
            if(fromTime != null)
                counterService.Restart(fromTime.Value.Ticks);
            else
                counterService.Restart(null);
        }

        private void GetTaskWithAllDescendants(MonitoringTaskMetadata task, List<MonitoringTaskMetadata> list)
        {
            if(list.Count < maxCount) list.Add(task);
            else return;

            var monitoringTaskMetadatas = localStorage.Search<MonitoringTaskMetadata>(meta => meta.ParentTaskId == task.Id);
            foreach(var monitoringTaskMetadata in monitoringTaskMetadatas)
                GetTaskWithAllDescendants(monitoringTaskMetadata, list);
        }

        private readonly ILocalStorage localStorage;
        private readonly string taskMetaInfoTableName;
        private readonly ISqlDatabase sqlDatabase;
        private readonly ILocalStorageUpdater localStorageUpdater;
        private readonly IMonitoringServiceSettings monitoringServiceSettings;
        private readonly CounterService counterService;
        private const int maxCount = 300;
    }
}