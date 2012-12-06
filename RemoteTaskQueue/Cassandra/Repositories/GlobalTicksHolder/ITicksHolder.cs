namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    public interface ITicksHolder
    {
        long UpdateMaxTicks(string name, long ticks);
        long GetMaxTicks(string name);
        long UpdateMinTicks(string name, long ticks);
        long GetMinTicks(string name);
    }
}