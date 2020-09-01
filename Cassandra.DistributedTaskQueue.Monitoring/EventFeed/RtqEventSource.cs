using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeed
{
    internal class RtqEventSource : IEventSource<TaskMetaUpdatedEvent, string>
    {
        public RtqEventSource(IEventLogRepository eventLogRepository)
        {
            this.eventLogRepository = eventLogRepository;
        }

        [NotNull]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        [NotNull]
        public EventsQueryResult<TaskMetaUpdatedEvent, string> GetEvents([CanBeNull] string fromOffsetExclusive, [NotNull] string toOffsetInclusive, int estimatedCount)
        {
            if (estimatedCount <= 0)
                throw new InvalidOperationException("estimatedCount <= 0");
            if (string.IsNullOrEmpty(toOffsetInclusive))
                throw new InvalidOperationException("toOffsetInclusive is not set");

            var firstEventTicks = eventLogRepository.GetFirstEventTicks();
            if (firstEventTicks == 0)
                return new EventsQueryResult<TaskMetaUpdatedEvent, string>(new List<EventWithOffset<TaskMetaUpdatedEvent, string>>(), lastOffset : null, noMoreEventsInSource : true);

            var exclusiveStartColumnName = GetExclusiveStartColumnName(fromOffsetExclusive, firstEventTicks);
            if (EventPointerFormatter.CompareColumnNames(exclusiveStartColumnName, toOffsetInclusive) >= 0)
                return new EventsQueryResult<TaskMetaUpdatedEvent, string>(new List<EventWithOffset<TaskMetaUpdatedEvent, string>>(), lastOffset : null, noMoreEventsInSource : true);

            if (estimatedCount == int.MaxValue)
                estimatedCount--;

            var eventsToFetch = estimatedCount;
            var events = new List<EventWithOffset<TaskMetaUpdatedEvent, string>>();
            var partitionKey = EventPointerFormatter.GetPartitionKey(EventPointerFormatter.GetTimestamp(exclusiveStartColumnName).Ticks);
            while (true)
            {
                var eventsBatch = eventLogRepository.GetEvents(partitionKey, exclusiveStartColumnName, toOffsetInclusive, eventsToFetch, out var currentPartitionIsExhausted);
                events.AddRange(eventsBatch.Select(x => new EventWithOffset<TaskMetaUpdatedEvent, string>(x.Event, x.Offset)));

                if (!currentPartitionIsExhausted)
                    return new EventsQueryResult<TaskMetaUpdatedEvent, string>(events, lastOffset : events.Last().Offset, noMoreEventsInSource : false);

                var nextPartitionStartTicks = (EventPointerFormatter.ParsePartitionKey(partitionKey) + 1) * EventPointerFormatter.PartitionDurationTicks;
                if (nextPartitionStartTicks > EventPointerFormatter.GetTimestamp(toOffsetInclusive).Ticks)
                    return new EventsQueryResult<TaskMetaUpdatedEvent, string>(events, lastOffset : toOffsetInclusive, noMoreEventsInSource : true);

                eventsToFetch = estimatedCount - events.Count;
                partitionKey = EventPointerFormatter.GetPartitionKey(nextPartitionStartTicks);
                exclusiveStartColumnName = EventPointerFormatter.GetMaxColumnNameForTicks(nextPartitionStartTicks - 1);
            }
        }

        [NotNull]
        private static string GetExclusiveStartColumnName([CanBeNull] string fromOffsetExclusive, long firstEventTicks)
        {
            if (!string.IsNullOrEmpty(fromOffsetExclusive))
            {
                var fromTimestampExclusive = EventPointerFormatter.GetTimestamp(fromOffsetExclusive);
                if (fromTimestampExclusive.Ticks >= firstEventTicks)
                    return fromOffsetExclusive;
            }
            return EventPointerFormatter.GetMaxColumnNameForTicks(firstEventTicks - 1);
        }

        private readonly IEventLogRepository eventLogRepository;
    }
}