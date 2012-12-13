using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.Queries;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class MonitoringServiceImpl : IMonitoringServiceImpl
    {
        public MonitoringServiceImpl(ILocalStorage localStorage, IRemoteTaskQueue remoteTaskQueue, ILocalStorageTableRegistry localStorageTableRegistry, ISqlDatabase sqlDatabase)
        {
            this.localStorage = localStorage;
            this.remoteTaskQueue = remoteTaskQueue;
            this.sqlDatabase = sqlDatabase;
            taskMetaInfoTableName = localStorageTableRegistry.GetTableName(typeof(TaskMetaInformationBusinessObjectWrap));
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
            return localStorage.GetCount<TaskMetaInformationBusinessObjectWrap>(getCountQuery.Criterion);
        }

        public TaskMetaInformation[] Search(MonitoringSearchQuery searchQuery)
        {
            return localStorage.Search<TaskMetaInformationBusinessObjectWrap>(searchQuery.Criterion, searchQuery.RangeFrom, searchQuery.Count, searchQuery.SortRules).Select(x => x.Info).ToArray();
        }

        public object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery)
        {
            var sqlSelectQuery = new SqlSelectQuery
                {
                    Criterion = getDistinctValuesQuery.Criterion,
                    NeedColumns = new[] {getDistinctValuesQuery.ColumnPath.Path},
                    Distinct = true,
                    TableName = taskMetaInfoTableName
                };
            var debagRes = sqlDatabase.GetRows(sqlSelectQuery).Select(x => x[0]).ToArray();
            return sqlDatabase.GetRows(sqlSelectQuery).Select(x => x[0]).ToArray();
        }

        public bool CancelTask(string taskId)
        {
            return remoteTaskQueue.CancelTask(taskId);
        }

        public RemoteTaskInfo GetTaskInfo(string taskId)
        {
            return remoteTaskQueue.GetTaskInfo(taskId);
        }

        private readonly ILocalStorage localStorage;
        private readonly string taskMetaInfoTableName;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ISqlDatabase sqlDatabase;
    }
}