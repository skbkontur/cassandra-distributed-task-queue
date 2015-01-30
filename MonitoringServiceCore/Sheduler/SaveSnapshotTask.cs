using System;

using RemoteQueue.LocalTasks.Scheduling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler
{
    public class SaveSnapshotTask : IPeriodicTask
    {
        public SaveSnapshotTask(CounterService counterService)
        {
            if(counterService == null) throw new ArgumentNullException("counterService");
            this.counterService = counterService;
        }

        public string Id { get { return GetType().Name; } }

        public void Run()
        {
            counterService.SaveSnapshot();
        }

        private readonly CounterService counterService;
    }
}