using RemoteQueue.LocalTasks.Scheduling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler
{
    public class CounterUpdaterTask : IPeriodicTask
    {
        public CounterUpdaterTask(CounterService counterService)
        {
            this.counterService = counterService;
        }

        public string Id { get { return GetType().Name; } }

        public void Run()
        {
            counterService.Update();
        }

        private readonly CounterService counterService;
    }
}