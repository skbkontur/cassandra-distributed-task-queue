using System.Linq;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.Queries;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class MonitoringServiceImpl : IMonitoringServiceImpl
    {
        public MonitoringServiceImpl(ILocalStorage localStorage, ILocalStorageTableRegistry localStorageTableRegistry, ISqlDatabase sqlDatabase, ILocalStorageUpdater localStorageUpdater)
        {
            this.localStorage = localStorage;
            this.sqlDatabase = sqlDatabase;
            this.localStorageUpdater = localStorageUpdater;
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
            localStorageUpdater.UpdateLocalStorage();
            return localStorage.GetCount<TaskMetaInformationBusinessObjectWrap>(getCountQuery.Criterion);
        }

        public TaskMetaInformation[] Search(MonitoringSearchQuery searchQuery)
        {
            localStorageUpdater.UpdateLocalStorage();
            var taskMetaInformations = localStorage.Search<TaskMetaInformationBusinessObjectWrap>(searchQuery.Criterion, searchQuery.RangeFrom, searchQuery.Count, searchQuery.SortRules).Select(x => x.Info).ToArray();
            return taskMetaInformations;
        }

        public object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery)
        {
            localStorageUpdater.UpdateLocalStorage();
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

        private readonly ILocalStorage localStorage;
        private readonly string taskMetaInfoTableName;
        private readonly ISqlDatabase sqlDatabase;
        private readonly ILocalStorageUpdater localStorageUpdater;
    }
}