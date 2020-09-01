using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    public interface IRtqTaskCounterStateStorage
    {
        [CanBeNull]
        byte[] TryRead();

        void Write([NotNull] byte[] serializedState);
    }
}