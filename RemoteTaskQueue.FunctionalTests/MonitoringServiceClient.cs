using GroBuf;

using RemoteTaskQueue.Monitoring.TaskCounter;

using SKBKontur.Catalogue.ClientLib.GroboClusterClient;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.FunctionalTests
{
    public class MonitoringServiceClient : HttpClientForTestsBase
    {
        public MonitoringServiceClient(ISerializer serializer)
            : base(serializer, Log.For<MonitoringServiceClient>(), applicationName : "RtqMonitoringServiceClient", port : 4413)
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
    }
}