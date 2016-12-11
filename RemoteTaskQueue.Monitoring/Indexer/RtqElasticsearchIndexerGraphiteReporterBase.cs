using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Core.Graphite.Client.StatsD;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public abstract class RtqElasticsearchIndexerGraphiteReporterBase : IRtqElasticsearchIndexerGraphiteReporter
    {
        protected RtqElasticsearchIndexerGraphiteReporterBase([NotNull] string graphitePrefix, ICatalogueGraphiteClient graphiteClient, ICatalogueStatsDClient statsDClient)
        {
            this.graphitePrefix = graphitePrefix;
            this.graphiteClient = graphiteClient;
            this.statsDClient = statsDClient;
        }

        public void ReportActualizationLag(TimeSpan lag)
        {
            graphiteClient.Send(string.Format("{0}.ActualizationLagInSeconds", graphitePrefix), (long)lag.TotalSeconds, Timestamp.Now.ToDateTime());
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
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly ICatalogueStatsDClient statsDClient;
    }
}