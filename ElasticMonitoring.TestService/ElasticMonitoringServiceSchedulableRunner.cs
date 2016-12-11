using System;

using RemoteTaskQueue.Monitoring.Indexer;

using SKBKontur.Catalogue.ServiceLib.Logging;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class ElasticMonitoringServiceSchedulableRunner
    {
        public ElasticMonitoringServiceSchedulableRunner(IPeriodicTaskRunner periodicTaskRunner, ITaskIndexController taskIndexController)
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.taskIndexController = taskIndexController;
        }

        public void Stop()
        {
            if(started)
            {
                lock(lockObject)
                {
                    if(started)
                    {
                        periodicTaskRunner.Unregister(taskSearchUpdateTaskId, 15000);
                        started = false;
                        Log.For(this).Info("Stop MonitoringServiceSchedulableRunner");
                    }
                }
            }
        }

        public void Start()
        {
            if(!started)
            {
                lock(lockObject)
                {
                    if(!started)
                    {
                        periodicTaskRunner.Register(new ActionPeriodicTask(taskSearchUpdateTaskId, () => taskIndexController.ProcessNewEvents()), TimeSpan.FromSeconds(5));
                        started = true;
                        Log.For(this).Info("Start MonitoringServiceSchedulableRunner");
                    }
                }
            }
        }

        private const string taskSearchUpdateTaskId = "UpdateTaskSearchIndex";
        private volatile bool started;
        private readonly object lockObject = new object();
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly ITaskIndexController taskIndexController;
    }
}