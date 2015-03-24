using log4net;

using RemoteQueue.LocalTasks.Scheduling;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.TaskSearch;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Scheduler
{
    public class ElasticMonitoringServiceSchedulableRunner : IElasticMonitoringServiceSchedulableRunner
    {
        public ElasticMonitoringServiceSchedulableRunner(
            IPeriodicTaskRunner periodicTaskRunner,
            CurrentMetaProvider currentMetaProvider,
            TaskSearchConsumer taskSearchConsumer)
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.currentMetaProvider = currentMetaProvider;
            this.taskSearchConsumer = taskSearchConsumer;
        }

        public void Stop()
        {
            if(worked)
            {
                lock(lockObject)
                {
                    if(worked)
                    {
                        periodicTaskRunner.Unregister(fetchMetasTaskId, 15000);
                        periodicTaskRunner.Unregister(taskSearchUpdateTaskId, 15000);
                        worked = false;
                        logger.Info("Stop MonitoringServiceSchedulableRunner");
                    }
                }
            }
        }

        public void Start()
        {
            if(!worked)
            {
                lock(lockObject)
                {
                    if(!worked)
                    {
                        periodicTaskRunner.Register(new ActionPeriodicTask(() => currentMetaProvider.FetchMetas(), fetchMetasTaskId), MetaProviderSettings.FetchMetasInterval);
                        periodicTaskRunner.Register(new ActionPeriodicTask(() => taskSearchConsumer.ProcessQueue(), taskSearchUpdateTaskId), TaskSearchSettings.IndexInterval);
                        worked = true;
                        logger.InfoFormat("Start MonitoringServiceSchedulableRunner");
                    }
                }
            }
        }

        private const string taskSearchUpdateTaskId = "UpdateTaskSearchIndex";
        private const string fetchMetasTaskId = "fetchMetasTaskId";

        private readonly object lockObject = new object();
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly CurrentMetaProvider currentMetaProvider;
        private readonly TaskSearchConsumer taskSearchConsumer;

        private volatile bool worked;

        private readonly ILog logger = LogManager.GetLogger(typeof(ElasticMonitoringServiceSchedulableRunner));
    }
}