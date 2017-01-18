using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.Graphite.Client.StatsD;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public abstract class RtqElasticsearchIndexerGraphiteReporterBase : IRtqElasticsearchIndexerGraphiteReporter
    {
        protected RtqElasticsearchIndexerGraphiteReporterBase([NotNull] string graphitePrefix, ICatalogueStatsDClient statsDClient)
        {
            this.graphitePrefix = graphitePrefix;
            this.statsDClient = statsDClient;
        }

        public void ReportTiming([NotNull] string actionName, [NotNull] Action action)
        {
            statsDClient.Timing(string.Format("{0}.{1}", graphitePrefix, actionName), action);
        }

        public TResult ReportTiming<TResult>([NotNull] string actionName, [NotNull] Func<TResult> action)
        {
            return statsDClient.Timing(string.Format("{0}.{1}", graphitePrefix, actionName), action);
        }

        private readonly string graphitePrefix;
        private readonly ICatalogueStatsDClient statsDClient;
    }
}