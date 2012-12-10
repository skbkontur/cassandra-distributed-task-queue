using System.Linq;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Implementation
{
    public class MonitoringServiceImpl : IMonitoringServiceImpl
    {
        public MonitoringServiceImpl(ILocalStorage localStorage)
        {
            this.localStorage = localStorage;
        }

        public int GetCount()
        {
            return localStorage.GetCount<TaskMetaInformationBusinessObjectWrap>(x => true);
        }

        public TaskMetaInformation[] GetRange(int start, int count)
        {
            return localStorage.Search<TaskMetaInformationBusinessObjectWrap>(x => true, start, count).Select(wrap => wrap.Info).ToArray();
        }

        public void ActualizeDatabaseScheme()
        {
            localStorage.ActualizeDatabaseScheme();
        }

        public void DropLocalStorage()
        {
            localStorage.DropDatabase();
        }

        private readonly ILocalStorage localStorage;
    }
}