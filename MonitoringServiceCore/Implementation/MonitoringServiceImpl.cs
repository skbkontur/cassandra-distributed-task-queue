using System.Collections.Generic;
using System.Linq;

using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.Queries;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class MonitoringServiceImpl : IMonitoringServiceImpl
    {
        public MonitoringServiceImpl(ILocalStorage localStorage, ILocalStorageTableRegistry localStorageTableRegistry, ISqlDatabase sqlDatabase, ILocalStorageUpdater localStorageUpdater)
        {
            this.localStorage = localStorage;
            this.sqlDatabase = sqlDatabase;
            taskMetaInfoTableName = localStorageTableRegistry.GetTableName(typeof(MonitoringTaskMetadata));
            this.localStorageUpdater = localStorageUpdater;
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
            //localStorageUpdater.Update();
            return localStorage.GetCount<MonitoringTaskMetadata>(getCountQuery.Criterion);
        }

        public void RecalculateInProcess()
        {
            localStorageUpdater.RecalculateInProcess();
        }

        public MonitoringTaskMetadata[] Search(MonitoringSearchQuery searchQuery)
        {
            //localStorageUpdater.Update();
            return localStorage.Search<MonitoringTaskMetadata>(searchQuery.Criterion, searchQuery.RangeFrom, searchQuery.Count, searchQuery.SortRules);
        }

        public object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery)
        {
            //localStorageUpdater.Update();
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
        private const int maxCount = 300;
    }
}