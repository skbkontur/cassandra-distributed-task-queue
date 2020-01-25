using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Settings;

using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories
{
    public class EventLogRepository : ColumnFamilyRepositoryBase, IEventLogRepository, IEventSource<TaskMetaUpdatedEvent, string>
    {
        public EventLogRepository(ISerializer serializer, ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings settings, IMinTicksHolder minTicksHolder)
            : base(cassandraCluster, settings, ColumnFamilyName)
        {
            this.serializer = serializer;
            this.minTicksHolder = minTicksHolder;
            var connectionParameters = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, ColumnFamilyName).GetConnectionParameters();
            UnstableZoneLength = TimeSpan.FromMilliseconds(connectionParameters.Attempts * connectionParameters.Timeout);
        }

        public TimeSpan UnstableZoneLength { get; }

        [NotNull]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        public void AddEvent([NotNull] TaskMetaInformation taskMeta, [NotNull] Timestamp eventTimestamp, Guid eventId)
        {
            minTicksHolder.UpdateMinTicks(firstEventTicksRowName, eventTimestamp.Ticks);
            var ttl = taskMeta.GetTtl();
            RetrieveColumnFamilyConnection().AddColumn(EventPointerFormatter.GetPartitionKey(eventTimestamp.Ticks), new Column
                {
                    Name = EventPointerFormatter.GetColumnName(eventTimestamp.Ticks, eventId),
                    Value = serializer.Serialize(new TaskMetaUpdatedEvent(taskMeta.Id, eventTimestamp.Ticks)),
                    Timestamp = eventTimestamp.Ticks,
                    TTL = ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int?)null,
                });
        }

        [NotNull]
        public EventsQueryResult<TaskMetaUpdatedEvent, string> GetEvents([CanBeNull] string fromOffsetExclusive, [NotNull] string toOffsetInclusive, int estimatedCount)
        {
            if (estimatedCount <= 0)
                throw new InvalidProgramStateException("estimatedCount <= 0");
            if (string.IsNullOrEmpty(toOffsetInclusive))
                throw new InvalidProgramStateException("toOffsetInclusive is not set");
            var firstEventTicks = minTicksHolder.GetMinTicks(firstEventTicksRowName);
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
                var columnsToFetch = eventsToFetch + 1;
                var columnsIncludingStartColumn = RetrieveColumnFamilyConnection().GetColumns(partitionKey, exclusiveStartColumnName, toOffsetInclusive, columnsToFetch, reversed : false);
                events.AddRange(columnsIncludingStartColumn.SkipWhile(x => x.Name == exclusiveStartColumnName)
                                                           .Select(x => new EventWithOffset<TaskMetaUpdatedEvent, string>(serializer.Deserialize<TaskMetaUpdatedEvent>(x.Value), offset : x.Name))
                                                           .Take(eventsToFetch));
                var currentPartitionIsExhausted = columnsIncludingStartColumn.Length < columnsToFetch;
                if (!currentPartitionIsExhausted)
                    return new EventsQueryResult<TaskMetaUpdatedEvent, string>(events, lastOffset : events.Last().Offset, noMoreEventsInSource : false);
                else
                {
                    var nextPartitionStartTicks = (EventPointerFormatter.ParsePartitionKey(partitionKey) + 1) * EventPointerFormatter.PartitionDurationTicks;
                    if (nextPartitionStartTicks > EventPointerFormatter.GetTimestamp(toOffsetInclusive).Ticks)
                        return new EventsQueryResult<TaskMetaUpdatedEvent, string>(events, lastOffset : toOffsetInclusive, noMoreEventsInSource : true);
                    else
                    {
                        eventsToFetch = estimatedCount - events.Count;
                        partitionKey = EventPointerFormatter.GetPartitionKey(nextPartitionStartTicks);
                        exclusiveStartColumnName = EventPointerFormatter.GetColumnName(nextPartitionStartTicks - 1, GuidHelpers.MaxGuid);
                    }
                }
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
            return EventPointerFormatter.GetColumnName(firstEventTicks - 1, GuidHelpers.MaxGuid);
        }

        public const string ColumnFamilyName = "RemoteTaskQueueEventLog";
        private const string firstEventTicksRowName = "firstEventTicksRowName";
        private readonly ISerializer serializer;
        private readonly IMinTicksHolder minTicksHolder;
    }
}