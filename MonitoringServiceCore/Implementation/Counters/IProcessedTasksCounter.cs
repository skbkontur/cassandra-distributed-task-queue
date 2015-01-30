using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public interface IProcessedTasksCounter : IMetaConsumer
    {
        TaskCount GetCount();
        void Reset();
        ProcessedTasksCounter.CounterSnapshot GetSnapshotOrNull(int maxLength);
        void LoadSnapshot(ProcessedTasksCounter.CounterSnapshot snapshot);
    }
}