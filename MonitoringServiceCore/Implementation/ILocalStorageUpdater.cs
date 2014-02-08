namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public interface ILocalStorageUpdater
    {
        void RecalculateInProcess();
        void Update();
        void ClearProcessedEvents();
    }
}