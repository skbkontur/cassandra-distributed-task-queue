using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.Topology;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public class MonitoringServiceClient : IMonitoringServiceClient
    {
        public MonitoringServiceClient(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory)
        {
            this.domainTopologyFactory = domainTopologyFactory;
            this.methodDomainFactory = methodDomainFactory;
            domainTopology = domainTopologyFactory.Create("remoteTaskQueueMonitoringServiceTopology");
        }

        public int GetCount()
        {
            var domain = methodDomainFactory.Create("GetCount", domainTopology, timeout, clientName);
            return domain.QueryFromRandomReplica<int>();
        }

        public TaskMetaInformation[] GetRange(int start, int count)
        {
            var domain = methodDomainFactory.Create("GetRange", domainTopology, timeout, clientName);
            return domain.QueryFromRandomReplica<TaskMetaInformation[], int, int>(start, count);
        }

        public void ActualizeDatabaseScheme()
        {
            var domain = methodDomainFactory.Create("ActualizeDatabaseScheme", domainTopology, timeout, clientName);
            domain.SendToEachReplica(DomainConsistencyLevel.All);
        }

        public bool CancelTask(string taskId)
        {
            var domain = methodDomainFactory.Create("CancelTask", domainTopology, timeout, clientName);
            return domain.QueryFromRandomReplica<bool, string>(taskId);
        }

        public RemoteTaskInfo GetTaskInfo(string taskId)
        {
            var domain = methodDomainFactory.Create("GetTaskInfo", domainTopology, timeout, clientName);
            return domain.QueryFromRandomReplica<RemoteTaskInfo, string>(taskId);
        }

        public void DropLocalStorage()
        {
            var domain = methodDomainFactory.Create("DropLocalStorage", domainTopology, timeout, clientName);
            domain.SendToEachReplica(DomainConsistencyLevel.All);
        }

        private readonly IDomainTopologyFactory domainTopologyFactory;
        private readonly IMethodDomainFactory methodDomainFactory;
        private readonly IDomainTopology domainTopology;
        private const int timeout = 30 * 1000;
        private const string clientName = "RemoteTaskQueueMonitoringServiceClient";
    }
}