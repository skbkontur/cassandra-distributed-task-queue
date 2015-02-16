using System;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.Queries;
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
        public int GetCount(MonitoringGetCountQuery monitoringGetCountQuery)
        {
            return monitoringServiceImpl.GetCount(monitoringGetCountQuery);
        }

        [HttpMethod]
        public TaskCount GetProcessingTaskCount()
        {
            return monitoringServiceImpl.GetProcessingTaskCount();
        }

        [HttpMethod]
        public void RestartProcessgingTaskCounter(DateTime? fromTime)
        {
            monitoringServiceImpl.RestartProcessgingTaskCounter(fromTime);
        }

        [HttpMethod]
        public MonitoringTaskMetadata[] Search(MonitoringSearchQuery searchQuery)
        {
            return monitoringServiceImpl.Search(searchQuery);
        }

        [HttpMethod]
        public int RecalculateInProcess()
        {
            monitoringServiceImpl.RecalculateInProcess();
            return 0;
        }

        [HttpMethod]
        public object[] GetDistinctValues(MonitoringGetDistinctValuesQuery getDistinctValuesQuery)
        {
            return monitoringServiceImpl.GetDistinctValues(getDistinctValuesQuery);
        }

        [HttpMethod]
        public MonitoringTaskMetadata[] GetTaskWithAllDescendants(string taskId)
        {
            return monitoringServiceImpl.GetTaskWithAllDescendants(taskId);
        }

        [HttpMethod]
        [JsonHttpMethod]
        public ActualizationStateInfo ActualizationState()
        {
            return new ActualizationStateInfo
                {
                    Lag = monitoringServiceImpl.GetActualizationLag()
                };
        }

        [HttpMethod]
        public string State()
        {
            return "Started";
        }

        public class ActualizationStateInfo
        {
            public TimeSpan Lag { get; set; }
        }

        private readonly IMonitoringServiceImpl monitoringServiceImpl;
    }
}