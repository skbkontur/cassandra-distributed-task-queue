using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    public class MonitoringServiceClient : HttpClientForTestsBase
    {
        public MonitoringServiceClient()
            : base(devPort : 4413, defaultRequestTimeout : TimeSpan.FromMinutes(1))
        {
        }

        public RtqTaskCountersForTests GetTaskCounters()
        {
            return Post("GetTaskCounters").ThenReturn<RtqTaskCountersForTests>();
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