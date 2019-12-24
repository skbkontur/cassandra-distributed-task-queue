namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    public interface IGlobalTime
    {
        long UpdateNowTicks();
        void ResetInMemoryState();
    }
}