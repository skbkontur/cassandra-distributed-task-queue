using System;

using RemoteTaskQueue.Monitoring.Storage.Utils;

using SKBKontur.Catalogue.Objects.Json;
using SKBKontur.Catalogue.ServiceLib.Logging;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqElasticsearchIndexerAuxilaryJobsRunner
    {
        public RtqElasticsearchIndexerAuxilaryJobsRunner(IPeriodicTaskRunner periodicTaskRunner, IRtqElasticsearchIndexer indexer, IRtqElasticsearchIndexerGraphiteReporter graphiteReporter)
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.indexer = indexer;
            this.graphiteReporter = graphiteReporter;
        }

        public void Start()
        {
            periodicTaskRunner.Register(reportIndexingProgress, period : TimeSpan.FromMinutes(1), taskAction : () =>
                {
                    var status = indexer.GetStatus();
                    Log.For(this).LogInfoFormat("Status: {0}", status.ToPrettyJson());
                    graphiteReporter.ReportActualizationLag(status.ActualizationLag);
                });
        }

        public void Stop()
        {
            periodicTaskRunner.Unregister(reportIndexingProgress, TimeSpan.FromSeconds(10));
        }

        private const string reportIndexingProgress = "ReportIndexingProgress";
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IRtqElasticsearchIndexer indexer;
        private readonly IRtqElasticsearchIndexerGraphiteReporter graphiteReporter;
    }
}