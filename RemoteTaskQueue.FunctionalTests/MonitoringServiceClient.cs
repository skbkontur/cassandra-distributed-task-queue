using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

namespace RemoteTaskQueue.FunctionalTests
{
    public class MonitoringServiceClient : HttpClientForTestsBase
    {
        public MonitoringServiceClient()
            : base(devPort : 4413, defaultRequestTimeout : TimeSpan.FromMinutes(1))
        {
        }

        public RtqTaskCounters GetTaskCounters()
        {
            return Post("GetTaskCounters").ThenReturn<RtqTaskCounters>();
        }

        public void ExecuteForcedFeeding()
        {
            Post("ExecuteForcedFeeding");
        }

        public void ResetState()
        {
            Post("ResetState");
        }

        public void Stop()
        {
            Post("Stop");
        }
    }
}