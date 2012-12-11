using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Http
{
    public class MonitoringServiceHttpHandler : IHttpHandler
    {
        public MonitoringServiceHttpHandler(IMonitoringServiceImpl monitoringServiceImpl)
        {
            this.monitoringServiceImpl = monitoringServiceImpl;
        }

        [HttpMethod]
        public int GetCount()
        {
            return monitoringServiceImpl.GetCount();
        }

        [HttpMethod]
        public TaskMetaInformation[] GetRange(int start, int count)
        {
            return monitoringServiceImpl.GetRange(start, count);
        }

        [HttpMethod]
        public void ActualizeDatabaseScheme()
        {
            monitoringServiceImpl.ActualizeDatabaseScheme();
        }

        [HttpMethod]
        public void DropLocalStorage()
        {
            monitoringServiceImpl.DropLocalStorage();
        }

        [HttpMethod]
        public bool CancelTask(string taskId)
        {
            return monitoringServiceImpl.CancelTask(taskId);
        }

        [HttpMethod]
        public RemoteTaskInfo GetTaskInfo(string taskId)
        {
            return monitoringServiceImpl.GetTaskInfo(taskId);
        }

        private readonly IMonitoringServiceImpl monitoringServiceImpl;
    }
}