using JetBrains.Annotations;

namespace RemoteTaskQueue.Monitoring.TaskCounter
{
    public interface IRtqTaskCounterStateStorage
    {
        [CanBeNull]
        byte[] TryRead();

        void Write([NotNull] byte[] serializedState);
    }
}