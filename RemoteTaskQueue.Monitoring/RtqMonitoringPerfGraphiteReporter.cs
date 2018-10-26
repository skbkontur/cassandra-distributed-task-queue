using System;
using System.Diagnostics;

using JetBrains.Annotations;

using SkbKontur.Graphite.Client;

namespace RemoteTaskQueue.Monitoring
{
    public class RtqMonitoringPerfGraphiteReporter
    {
        public RtqMonitoringPerfGraphiteReporter([NotNull] string graphitePrefix, IStatsDClient statsDClient)
        {
            this.graphitePrefix = graphitePrefix;
            this.statsDClient = statsDClient;
        }

        public void ReportTiming([NotNull] string actionName, [NotNull] Action action)
        {
            ReportTiming(actionName, action, out _);
        }

        public void ReportTiming([NotNull] string actionName, [NotNull] Action action, [NotNull] out Stopwatch timer)
        {
            statsDClient.Timing($"{graphitePrefix}.{actionName}", action, out timer);
        }

        public TResult ReportTiming<TResult>([NotNull] string actionName, [NotNull] Func<TResult> action)
        {
            return ReportTiming(actionName, action, out _);
        }

        public TResult ReportTiming<TResult>([NotNull] string actionName, [NotNull] Func<TResult> action, [NotNull] out Stopwatch timer)
        {
            return statsDClient.Timing($"{graphitePrefix}.{actionName}", action, out timer);
        }

        public void Increment([NotNull] string counterName, int magnitude)
        {
            statsDClient.Increment($"{graphitePrefix}.{counterName}", magnitude);
        }

        private readonly string graphitePrefix;
        private readonly IStatsDClient statsDClient;
    }
}