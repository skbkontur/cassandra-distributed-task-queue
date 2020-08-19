using System;
using System.Linq;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    public class RtqTaskCounterSettings
    {
        public RtqTaskCounterSettings([NotNull] string eventFeedKey, [NotNull] string perfGraphitePathPrefix)
        {
            if (string.IsNullOrEmpty(eventFeedKey))
                throw new InvalidOperationException("eventFeedKey is empty");
            if (string.IsNullOrEmpty(perfGraphitePathPrefix))
                throw new InvalidOperationException("perfGraphitePathPrefix is empty");
            EventFeedKey = eventFeedKey;
            PerfGraphitePathPrefix = perfGraphitePathPrefix;
        }

        [NotNull]
        public string EventFeedKey { get; }

        [NotNull]
        public string PerfGraphitePathPrefix { get; }

        [NotNull]
        public TimeSpan[] BladeDelays { get; set; } =
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(15)
            };

        public TimeSpan DelayBetweenEventFeedingIterations { get; set; } = TimeSpan.FromSeconds(1);

        public TimeSpan StatePersistingInterval { get; set; } = TimeSpan.FromMinutes(1);

        public TimeSpan PendingTaskExecutionUpperBound { get; set; } = TimeSpan.FromMinutes(30);

        public TimeSpan StateGarbageTtl => TimeSpan.FromTicks(BladeDelays.Max().Ticks * 2);
    }
}