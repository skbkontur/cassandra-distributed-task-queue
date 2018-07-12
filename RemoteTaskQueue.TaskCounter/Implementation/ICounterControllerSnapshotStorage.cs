namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public interface ICounterControllerSnapshotStorage
    {
        void SaveSnapshot(CounterControllerSnapshot snapshot);
        CounterControllerSnapshot ReadSnapshotOrNull();
    }
}