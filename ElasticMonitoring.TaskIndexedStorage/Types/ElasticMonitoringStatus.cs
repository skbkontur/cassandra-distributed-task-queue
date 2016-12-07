using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types
{
    public class ElasticMonitoringStatus
    {
        public long MinTicksHack { get; set; }
        public long UnprocessedMapLength { get; set; }
        public long ProcessedMapLength { get; set; }
        public TimeSpan? ActualizationLag { get; set; }
        public long LastTicks { get; set; }
        public long NowTicks { get; set; }
        public long SnapshotTicks { get; set; }
        public long MetaCacheSize { get; set; }
    }
}