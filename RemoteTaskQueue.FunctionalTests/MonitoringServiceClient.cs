using System;

using GroBuf;

using RemoteTaskQueue.Monitoring.TaskCounter;

using SKBKontur.Catalogue.ServiceLib.GroboClusterClient;

namespace RemoteTaskQueue.FunctionalTests
{
    public class MonitoringServiceClient : HttpClientForTestsBase
    {
        public MonitoringServiceClient(ISerializer serializer)
            : base(serializer, "RtqMonitoringServiceClient", port : 4413)
        {
        }

        public void Start()
        {
            clusterClient.Post("Start", new RequestSettings(timeout : TimeSpan.FromSeconds(30)));
        }

        public void Stop()
        {
            clusterClient.Post("Stop", new RequestSettings(timeout : TimeSpan.FromMinutes(2)));
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
    }
}