namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public interface ICounterControllerSnapshotStorage
    {
        void SaveSnapshot(CounterControllerSnapshot snapshot);
        CounterControllerSnapshot ReadSnapshotOrNull();
    }
}