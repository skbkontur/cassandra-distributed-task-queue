namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    public interface IGlobalTime
    {
        long UpdateNowTicks();
        long GetNowTicks();
        void ResetInMemoryState();
    }
}