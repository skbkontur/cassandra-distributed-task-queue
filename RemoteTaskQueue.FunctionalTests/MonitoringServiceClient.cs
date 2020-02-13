using System;

using GroBuf;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

using SKBKontur.Catalogue.ServiceLib.GroboClusterClient;

namespace RemoteTaskQueue.FunctionalTests
{
    public class MonitoringServiceClient : HttpClientForTestsBase
    {
        public MonitoringServiceClient(ISerializer serializer)
            : base(serializer, "RtqMonitoringServiceClient", port : 4413, defaultRequestTimeout : TimeSpan.FromMinutes(1))
        {
        }

        public RtqTaskCounters GetTaskCounters()
        {
            return clusterClient.Post("GetTaskCounters").ThenReturn<RtqTaskCounters>();
        }

        public void ExecuteForcedFeeding()
        {
            clusterClient.Post("ExecuteForcedFeeding");
        }

        public void ResetState()
        {
            clusterClient.Post("ResetState");
        }

        public void Stop()
        {
            clusterClient.Post("Stop");
        }
    }
}