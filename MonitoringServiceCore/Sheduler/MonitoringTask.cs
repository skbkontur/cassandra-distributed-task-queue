using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler
{
    public class MonitoringTask : IMonitoringTask
    {
        public MonitoringTask(ILocalStorageUpdater localStorageUpdater)
        {
            this.localStorageUpdater = localStorageUpdater;
        }

        public string Id { get { return "MonitoringTask"; } }

        public void Run()
        {
            localStorageUpdater.UpdateLocalStorage();
        }

        private readonly ILocalStorageUpdater localStorageUpdater;
    }
}