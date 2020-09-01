using System;
using System.Linq;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    public class RtqTaskCounterSettings
    {
        public RtqTaskCounterSettings([NotNull] string eventFeedKey,
                                      [NotNull] string rtqGraphitePathPrefix)
        {
            if (string.IsNullOrEmpty(eventFeedKey))
                throw new InvalidOperationException("eventFeedKey is empty");
            if (string.IsNullOrEmpty(rtqGraphitePathPrefix))
                throw new InvalidOperationException("rtqGraphitePathPrefix is empty");

            EventFeedKey = eventFeedKey;
            RtqGraphitePathPrefix = rtqGraphitePathPrefix;
        }

        [NotNull]
        public string EventFeedKey { get; }

        [NotNull]
        public string RtqGraphitePathPrefix { get; }

        [NotNull]
        public string PerfGraphitePathPrefix => $"{RtqGraphitePathPrefix}.TaskCounter.RtqPerf";

        [NotNull]
        public string EventFeedGraphitePathPrefix => $"{RtqGraphitePathPrefix}.TaskCounter.EventFeed";

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