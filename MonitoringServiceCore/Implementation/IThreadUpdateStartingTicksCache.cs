namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public interface IThreadUpdateStartingTicksCache
    {
        void Add(string guid, long startingTicks);
        void Remove(string guid);
        long GetMinimum();
    }
}