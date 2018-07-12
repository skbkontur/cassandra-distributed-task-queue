using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    public interface ITicksHolder
    {
        void UpdateMaxTicks([NotNull] string name, long ticks);
        long GetMaxTicks([NotNull] string name);
        void UpdateMinTicks([NotNull] string name, long ticks);
        long GetMinTicks([NotNull] string name);
        void ResetInMemoryState();
    }
}