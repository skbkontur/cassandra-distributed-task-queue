using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IMinTicksHolder
    {
        long GetMinTicks([NotNull] string name);
        void UpdateMinTicks([NotNull] string name, long ticks);
        void ResetInMemoryState();
    }
}