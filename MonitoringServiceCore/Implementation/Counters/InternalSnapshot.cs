namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public class InternalSnapshot
    {
        public InternalSnapshot(MetaProvider.MetaProviderSnapshot metaProviderSnapshot, ProcessedTasksCounter.CounterSnapshot counterSnapshot)
        {
            MetaProviderSnapshot = metaProviderSnapshot;
            CounterSnapshot = counterSnapshot;
        }

        public MetaProvider.MetaProviderSnapshot MetaProviderSnapshot { get; set; }
        public ProcessedTasksCounter.CounterSnapshot CounterSnapshot { get; set; }
    }
}