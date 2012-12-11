using RemoteQueue.LocalTasks.Scheduling;

using log4net;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler
{
    public class MonitoringServiceSchedulableRunner : IMonitoringServiceSchedulableRunner
    {
        public MonitoringServiceSchedulableRunner(IMonitoringSchedulableRunnerSettings runnerSettings, IMonitoringTask monitoringTask, IPeriodicTaskRunner periodicTaskRunner)
        {
            this.runnerSettings = runnerSettings;
            this.monitoringTask = monitoringTask;
            this.periodicTaskRunner = periodicTaskRunner;
        }

        public void Stop()
        {
            if(worked)
            {
                lock(lockObject)
                {
                    if(worked)
                    {
                        periodicTaskRunner.Unregister(monitoringTask.Id, 15000);
                        worked = false;
                        logger.Info("Stop MonitoringService");
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
                        periodicTaskRunner.Register(monitoringTask, runnerSettings.PeriodicInterval);
                        worked = true;
                        logger.InfoFormat("Start MonitoringShedulableRunner: schedule monitoringTask with period {0}", runnerSettings.PeriodicInterval);
                    }
                }
            }
        }

        private readonly IMonitoringSchedulableRunnerSettings runnerSettings;
        private readonly IPeriodicTask monitoringTask;
        private readonly object lockObject = new object();
        private readonly IPeriodicTaskRunner periodicTaskRunner;

        private volatile bool worked;

        private readonly ILog logger = LogManager.GetLogger(typeof(MonitoringServiceSchedulableRunner));
    }
}