namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types
{
    public class ElasticMonitoringStatus
    {
        public bool DistributedLockAcquired { get; set; }
        public long MinTicksHack { get; set; }
        public long UnprocessedMapLength { get; set; }
        public long ProcessedMapLength { get; set; }
        public long? ActualizationLagMs { get; set; }
        public long LastTicks { get; set; }
        public long NowTicks { get; set; }
        public long SnapshotTicks { get; set; }
    }
}