using System;
using System.Diagnostics;

using JetBrains.Annotations;

using SkbKontur.Graphite.Client;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring
{
    public class RtqMonitoringPerfGraphiteReporter
    {
        public RtqMonitoringPerfGraphiteReporter([NotNull] IStatsDClient statsDClient, [NotNull] string graphitePathPrefix)
        {
            this.statsDClient = statsDClient;
            this.graphitePathPrefix = graphitePathPrefix;
        }

        public void ReportTiming([NotNull] string actionName, [NotNull] Action action)
        {
            ReportTiming(actionName, action, out _);
        }

        public void ReportTiming([NotNull] string actionName, [NotNull] Action action, [NotNull] out Stopwatch timer)
        {
            statsDClient.Timing($"{graphitePathPrefix}.{actionName}", action, out timer);
        }

        public TResult ReportTiming<TResult>([NotNull] string actionName, [NotNull] Func<TResult> action)
        {
            return ReportTiming(actionName, action, out _);
        }

        public TResult ReportTiming<TResult>([NotNull] string actionName, [NotNull] Func<TResult> action, [NotNull] out Stopwatch timer)
        {
            return statsDClient.Timing($"{graphitePathPrefix}.{actionName}", action, out timer);
        }

        public void Increment([NotNull] string counterName, int magnitude)
        {
            statsDClient.Increment($"{graphitePathPrefix}.{counterName}", magnitude);
        }

        private readonly IStatsDClient statsDClient;
        private readonly string graphitePathPrefix;
    }
}