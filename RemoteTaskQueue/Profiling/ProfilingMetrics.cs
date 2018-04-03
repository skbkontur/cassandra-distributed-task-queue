using System.Collections;

using JetBrains.Annotations;

using Metrics;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Profiling
{
    internal static class ProfilingMetrics
    {
        static ProfilingMetrics()
        {
            rootContext = Metric.Context("RemoteTaskQueue").Context("Profiling");
        }

        [NotNull]
        public static Meter Meter([NotNull] this MetricsContext metricsContext, [NotNull] string meterName)
        {
            if (meterName.Contains("."))
                throw new InvalidProgramStateException($"Invalid meterName: {meterName}");
            var meterKey = $"{metricsContext.ContextName}-{meterName}";
            return meters.GetOrAddThreadSafely(meterKey, _ => rootContext.Context(metricsContext.ContextName).Meter(meterName, Unit.None, TimeUnit.Minutes));
        }

        [NotNull]
        public static Timer Timer([NotNull] this MetricsContext metricsContext, [NotNull] string timerName)
        {
            if (timerName.Contains("."))
                throw new InvalidProgramStateException($"Invalid timerName: {timerName}");
            var timerKey = $"{metricsContext.ContextName}-{timerName}";
            return timers.GetOrAddThreadSafely(timerKey, _ => rootContext.Context(metricsContext.ContextName).Timer(timerName, Unit.None, SamplingType.ExponentiallyDecaying, TimeUnit.Minutes, TimeUnit.Milliseconds));
        }

        private static readonly Metrics.MetricsContext rootContext;
        private static readonly Hashtable meters = new Hashtable();
        private static readonly Hashtable timers = new Hashtable();
    }
}