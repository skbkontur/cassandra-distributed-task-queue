using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService
{
    public interface IMonitoringService
    {
        public RtqTaskCounters GetTaskCounters();
        public void Stop();
        public void ExecuteForcedFeeding();
        public void ResetState();
    }
}