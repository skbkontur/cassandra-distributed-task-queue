using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.Queries;
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
        public int GetCount(MonitoringGetCountQuery getCountQuery)
        {
            return monitoringServiceImpl.GetCount(getCountQuery);
        }

        [HttpMethod]
        public TaskMetaInformation[] Search(MonitoringSearchQuery searchQuery)
        {
            return monitoringServiceImpl.Search(searchQuery);
        }

        [HttpMethod]
        public object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery)
        {
            return monitoringServiceImpl.GetDistinctValues(getDistinctValuesQuery);
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