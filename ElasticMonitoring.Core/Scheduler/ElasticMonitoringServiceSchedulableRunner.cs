using System;

using log4net;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

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
            if(started)
            {
                lock(lockObject)
                {
                    if(started)
                    {
                        periodicTaskRunner.Unregister(dumpStatusTaskId, 15000);
                        periodicTaskRunner.Unregister(sendactualizationlagtographiteTaskId, 15000);
                        periodicTaskRunner.Unregister(taskSearchUpdateTaskId, 15000);
                        started = false;
                        logger.Info("Stop MonitoringServiceSchedulableRunner");
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
                        periodicTaskRunner.Register(new ActionPeriodicTask(taskSearchUpdateTaskId, () => taskIndexController.ProcessNewEvents()), TaskIndexSettings.IndexInterval);
                        periodicTaskRunner.Register(new ActionPeriodicTask(sendactualizationlagtographiteTaskId, () => taskIndexController.SendActualizationLagToGraphite()), TimeSpan.FromMinutes(1));
                        periodicTaskRunner.Register(new ActionPeriodicTask(dumpStatusTaskId, () => taskIndexController.LogStatus()), TimeSpan.FromMinutes(1));
                        started = true;
                        logger.InfoFormat("Start MonitoringServiceSchedulableRunner");
                    }
                }
            }
        }

        private const string taskSearchUpdateTaskId = "UpdateTaskSearchIndex";
        private const string dumpStatusTaskId = "dumpStatusTask";
        private const string sendactualizationlagtographiteTaskId = "SendActualizationLagToGraphite";

        private readonly object lockObject = new object();
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly ITaskIndexController taskIndexController;

        private volatile bool started;

        private readonly ILog logger = LogManager.GetLogger(typeof(ElasticMonitoringServiceSchedulableRunner));
    }
}