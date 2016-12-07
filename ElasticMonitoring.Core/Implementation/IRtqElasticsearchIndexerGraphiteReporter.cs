using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public interface IRtqElasticsearchIndexerGraphiteReporter
    {
        void ReportActualizationLag(TimeSpan lag);
        void ReportTiming([NotNull] string actionName, [NotNull] Action action);
        TResult ReportTiming<TResult>([NotNull] string actionName, [NotNull] Func<TResult> action);
    }
}