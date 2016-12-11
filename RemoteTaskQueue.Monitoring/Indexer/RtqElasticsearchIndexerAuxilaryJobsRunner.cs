using System;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerAuxilaryJobsRunner
    {
        public RtqElasticsearchIndexerAuxilaryJobsRunner(IPeriodicTaskRunner periodicTaskRunner, ITaskIndexController taskIndexController)
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.taskIndexController = taskIndexController;
        }

        public void Start()
        {
            periodicTaskRunner.Register(reportIndexingProgress, period : TimeSpan.FromMinutes(1), taskAction : () =>
                {
                    taskIndexController.LogStatus();
                    taskIndexController.SendActualizationLagToGraphite();
                });
        }

        public void Stop()
        {
            periodicTaskRunner.Unregister(reportIndexingProgress, TimeSpan.FromSeconds(10));
        }

        private const string reportIndexingProgress = "ReportIndexingProgress";
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly ITaskIndexController taskIndexController;
    }
}