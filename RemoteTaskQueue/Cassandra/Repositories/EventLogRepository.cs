using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories
{
    public class EventLogRepository : ColumnFamilyRepositoryBase, IEventLogRepository, IEventSource<TaskMetaUpdatedEvent, string>
    {
        public EventLogRepository(ISerializer serializer, ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings settings, ITicksHolder ticksHolder)
            : base(cassandraCluster, settings, ColumnFamilyName)
        {
            this.serializer = serializer;
            this.ticksHolder = ticksHolder;
            var connectionParameters = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, ColumnFamilyName).GetConnectionParameters();
            UnstableZoneLength = TimeSpan.FromMilliseconds(connectionParameters.Attempts * connectionParameters.Timeout);
        }

        public TimeSpan UnstableZoneLength { get; private set; }

        [NotNull]
        public string GetDescription()
        {
            return GetType().FullName;
        }

        public void AddEvent([NotNull] TaskMetaInformation taskMeta, long nowTicks)
        {
            ticksHolder.UpdateMinTicks(firstEventTicksRowName, nowTicks);
            var ttl = taskMeta.GetTtl();
            var @event = new TaskMetaUpdatedEvent(taskMeta.Id, nowTicks);
            RetrieveColumnFamilyConnection().AddColumn(EventPointerFormatter.GetPartitionKey(@event.Ticks), new Column
                {
                    Name = EventPointerFormatter.GetColumnName(@event.Ticks, eventId : Guid.NewGuid()),
                    Timestamp = @event.Ticks,
                    Value = serializer.Serialize(@event),
                    TTL = ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int?)null,
                });
        }

        [NotNull]
        public EventsQueryResult<TaskMetaUpdatedEvent, string> GetEvents([CanBeNull] string fromOffsetExclusive, [NotNull] string toOffsetInclusive, int estimatedCount)
        {
            if(estimatedCount <= 0)
                throw new InvalidProgramStateException("estimatedCount <= 0");
            if(string.IsNullOrEmpty(toOffsetInclusive))
                throw new InvalidProgramStateException("toOffsetInclusive is not set");
            var firstEventTicks = ticksHolder.GetMinTicks(firstEventTicksRowName);
            if(firstEventTicks == 0)
                return new EventsQueryResult<TaskMetaUpdatedEvent, string>(new List<EventWithOffset<TaskMetaUpdatedEvent, string>>(), lastOffset : null, noMoreEventsInSource : true);
            var eventsToFetch = estimatedCount;
            if(eventsToFetch == int.MaxValue)
                eventsToFetch--;
            var events = new List<EventWithOffset<TaskMetaUpdatedEvent, string>>();
            var exclusiveStartColumnName = GetExclusiveStartColumnName(fromOffsetExclusive, firstEventTicks);
            var partitionKey = EventPointerFormatter.GetPartitionKey(EventPointerFormatter.GetTimestamp(exclusiveStartColumnName).Ticks);
            while(true)
            {
                var columnsToFetch = eventsToFetch + 1;
                var columnsIncludingStartColumn = RetrieveColumnFamilyConnection().GetColumns(partitionKey, exclusiveStartColumnName, toOffsetInclusive, columnsToFetch, reversed : false);
                events.AddRange(columnsIncludingStartColumn.SkipWhile(x => x.Name == exclusiveStartColumnName)
                                                           .Select(x => new EventWithOffset<TaskMetaUpdatedEvent, string>(serializer.Deserialize<TaskMetaUpdatedEvent>(x.Value), x.Name)));
                var currentPartitionIsExhausted = columnsIncludingStartColumn.Length < columnsToFetch;
                if(!currentPartitionIsExhausted)
                    return new EventsQueryResult<TaskMetaUpdatedEvent, string>(events, lastOffset : events.Last().Offset, noMoreEventsInSource : false);
                else
                {
                    var nextPartitionStartTicks = (EventPointerFormatter.ParsePartitionKey(partitionKey) + 1) * EventPointerFormatter.PartitionDurationTicks;
                    if(nextPartitionStartTicks > EventPointerFormatter.GetTimestamp(toOffsetInclusive).Ticks)
                        return new EventsQueryResult<TaskMetaUpdatedEvent, string>(events, lastOffset : toOffsetInclusive, noMoreEventsInSource : true);
                    else
                    {
                        eventsToFetch = eventsToFetch - events.Count;
                        partitionKey = EventPointerFormatter.GetPartitionKey(nextPartitionStartTicks);
                        exclusiveStartColumnName = EventPointerFormatter.GetColumnName(nextPartitionStartTicks - 1, GuidHelpers.MaxGuid);
        }
                }
            }
        }

        [NotNull]
        private static string GetExclusiveStartColumnName([CanBeNull] string fromOffsetExclusive, long firstEventTicks)
        {
            if(!string.IsNullOrEmpty(fromOffsetExclusive))
            {
                var fromTimestampExclusive = EventPointerFormatter.GetTimestamp(fromOffsetExclusive);
                if(fromTimestampExclusive.Ticks >= firstEventTicks)
                    return fromOffsetExclusive;
        }
            return EventPointerFormatter.GetColumnName(firstEventTicks - 1, GuidHelpers.MaxGuid);
        }

        public IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, long toTicks, int batchSize)
        {
            var firstEventTicks = ticksHolder.GetMinTicks(firstEventTicksRowName);
            if(firstEventTicks == 0)
                return new TaskMetaUpdatedEvent[0];
            firstEventTicks -= EventPointerFormatter.PartitionDurationTicks; //note что это ?
            fromTicks = new[] {0, fromTicks, firstEventTicks}.Max();
            return new GetEventLogEnumerable(serializer, RetrieveColumnFamilyConnection(), fromTicks, toTicks, batchSize);
        }

        public const string ColumnFamilyName = "RemoteTaskQueueEventLog";
        private const string firstEventTicksRowName = "firstEventTicksRowName";
        private readonly ISerializer serializer;
        private readonly ITicksHolder ticksHolder;
    }
}