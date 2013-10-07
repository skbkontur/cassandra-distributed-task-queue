using System.Collections.Generic;
using System.Linq;

using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.Queries;
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
            IMonitoringServiceSettings monitoringServiceSettings)
        {
            this.localStorage = localStorage;
            this.sqlDatabase = sqlDatabase;
            taskMetaInfoTableName = localStorageTableRegistry.GetTableName(typeof(MonitoringTaskMetadata));
            this.localStorageUpdater = localStorageUpdater;
            this.monitoringServiceSettings = monitoringServiceSettings;
        }

        public void ActualizeDatabaseScheme()
        {
            localStorage.ActualizeDatabaseScheme();
        }

        public void DropLocalStorage()
        {
            localStorage.DropDatabase();
        }

        public int GetCount(MonitoringGetCountQuery getCountQuery)
        {
            if(monitoringServiceSettings.ActualizeOnQuery)
                localStorageUpdater.Update();
            return localStorage.GetCount<MonitoringTaskMetadata>(getCountQuery.Criterion);
        }

        public void RecalculateInProcess()
        {
            localStorageUpdater.RecalculateInProcess();
        }

        public MonitoringTaskMetadata[] Search(MonitoringSearchQuery searchQuery)
        {
            if (monitoringServiceSettings.ActualizeOnQuery)
                localStorageUpdater.Update();
            return localStorage.Search<MonitoringTaskMetadata>(searchQuery.Criterion, searchQuery.RangeFrom, searchQuery.Count, searchQuery.SortRules);
        }

        public object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery)
        {
            if (monitoringServiceSettings.ActualizeOnQuery)
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
            if (monitoringServiceSettings.ActualizeOnQuery)
                localStorageUpdater.Update();
            var task = localStorage.Get<MonitoringTaskMetadata>(taskId, taskId);
            if(task == null)
                return null;
            return GetTaskWithAllDescendants(task, 0).ToArray();
        }

        private List<MonitoringTaskMetadata> GetTaskWithAllDescendants(MonitoringTaskMetadata task, int count)
        {
            var list = new List<MonitoringTaskMetadata> {task};
            count++;
            var monitoringTaskMetadatas = localStorage.Search<MonitoringTaskMetadata>(meta => meta.ParentTaskId == task.Id);
            foreach(var monitoringTaskMetadata in monitoringTaskMetadatas)
            {
                var range = GetTaskWithAllDescendants(monitoringTaskMetadata, count);
                list.AddRange(range);
                count += range.Count;
                if(count > maxCount)
                    return list;
            }
            return list;
        }

        private readonly ILocalStorage localStorage;
        private readonly string taskMetaInfoTableName;
        private readonly ISqlDatabase sqlDatabase;
        private readonly ILocalStorageUpdater localStorageUpdater;
        private readonly IMonitoringServiceSettings monitoringServiceSettings;
        private const int maxCount = 300;
    }
}