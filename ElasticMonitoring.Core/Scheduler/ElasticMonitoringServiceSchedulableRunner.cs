using log4net;

using RemoteQueue.LocalTasks.Scheduling;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Scheduler
{
    public class ElasticMonitoringServiceSchedulableRunner : IElasticMonitoringServiceSchedulableRunner
    {
        public ElasticMonitoringServiceSchedulableRunner(
            IPeriodicTaskRunner periodicTaskRunner,
            ITaskIndexController taskIndexController
            )
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.taskIndexController = taskIndexController;
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
                        periodicTaskRunner.Register(new ActionPeriodicTask(() => taskIndexController.ProcessNewEvents(), taskSearchUpdateTaskId), TaskIndexSettings.IndexInterval);
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
        private readonly ITaskIndexController taskIndexController;

        private volatile bool worked;

        private readonly ILog logger = LogManager.GetLogger(typeof(ElasticMonitoringServiceSchedulableRunner));
    }
}