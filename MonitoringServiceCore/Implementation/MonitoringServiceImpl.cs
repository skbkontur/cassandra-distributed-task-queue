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
            localStorageUpdater.UpdateLocalStorage();
            return localStorage.GetCount<MonitoringTaskMetadata>(getCountQuery.Criterion);
        }

        public MonitoringTaskMetadata[] Search(MonitoringSearchQuery searchQuery)
        {
            localStorageUpdater.UpdateLocalStorage();
            return localStorage.Search<MonitoringTaskMetadata>(searchQuery.Criterion, searchQuery.RangeFrom, searchQuery.Count, searchQuery.SortRules);
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
            //var debagRes = sqlDatabase.GetRows(sqlSelectQuery).Select(x => x[0]).ToArray();
            var debagRes = sqlDatabase.GetRows(sqlSelectQuery).Select(x => x[0]).ToArray();
            return debagRes;
        }

        private readonly ILocalStorage localStorage;
        private readonly string taskMetaInfoTableName;
        private readonly ISqlDatabase sqlDatabase;
        private readonly ILocalStorageUpdater localStorageUpdater;
    }
}