using System;

using SKBKontur.Catalogue.ServiceLib.Logging;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    public class MonitoringServiceSchedulableRunner
    {
        public MonitoringServiceSchedulableRunner(IPeriodicTaskRunner periodicTaskRunner, SynchronizedIndexer indexer)
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.indexer = indexer;
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
                        periodicTaskRunner.Register(new ActionPeriodicTask(taskSearchUpdateTaskId, () => indexer.ProcessNewEvents()), TimeSpan.FromSeconds(1));
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
        private readonly SynchronizedIndexer indexer;
    }
}