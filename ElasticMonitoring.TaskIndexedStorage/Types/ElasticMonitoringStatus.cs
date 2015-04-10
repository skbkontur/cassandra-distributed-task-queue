namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types
{
    public class ElasticMonitoringStatus
    {
        public bool IsProcessingQueue { get; set; }
        public bool DistributedLockAcquired { get; set; }
        public long MinTicksHack { get; set; }
    }
}