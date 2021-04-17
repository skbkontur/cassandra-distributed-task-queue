using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public interface IMonitoringService
    {
        public RtqTaskCounters GetTaskCounters();
        public void Stop();
        public void ExecuteForcedFeeding();
        public void ResetState();
    }
}