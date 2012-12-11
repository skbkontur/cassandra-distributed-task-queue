using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class MonitoringServiceImpl : IMonitoringServiceImpl
    {
        public MonitoringServiceImpl(ILocalStorage localStorage, IRemoteTaskQueue remoteTaskQueue)
        {
            this.localStorage = localStorage;
            this.remoteTaskQueue = remoteTaskQueue;
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

        public bool CancelTask(string taskId)
        {
            return remoteTaskQueue.CancelTask(taskId);
        }

        public RemoteTaskInfo GetTaskInfo(string taskId)
        {
            return remoteTaskQueue.GetTaskInfo(taskId);
        }

        private readonly ILocalStorage localStorage;
        private readonly IRemoteTaskQueue remoteTaskQueue;
    }
}