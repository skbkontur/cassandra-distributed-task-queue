using System;

using JetBrains.Annotations;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public interface IRtqElasticsearchIndexerGraphiteReporter
    {
        void ReportTiming([NotNull] string actionName, [NotNull] Action action);
        TResult ReportTiming<TResult>([NotNull] string actionName, [NotNull] Func<TResult> action);
    }
}