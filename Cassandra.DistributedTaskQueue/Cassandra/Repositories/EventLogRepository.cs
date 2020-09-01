using System;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.ThriftClient.Connections;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories
{
    internal class EventLogRepository : IEventLogRepository
    {
        public EventLogRepository(ISerializer serializer,
                                  ICassandraCluster cassandraCluster,
                                  IRtqSettings rtqSettings,
                                  IMinTicksHolder minTicksHolder)
        {
            this.serializer = serializer;
            this.minTicksHolder = minTicksHolder;
            cfConnection = cassandraCluster.RetrieveColumnFamilyConnection(rtqSettings.QueueKeyspace, ColumnFamilyName);
        }

        public TimeSpan GetUnstableZoneDuration()
        {
            var connectionParameters = cfConnection.GetConnectionParameters();
            var unstableZoneLength = TimeSpan.FromMilliseconds(connectionParameters.Attempts * connectionParameters.Timeout);
            return unstableZoneLength;
        }

        public void AddEvent([NotNull] TaskMetaInformation taskMeta, [NotNull] Timestamp eventTimestamp, Guid eventId)
        {
            minTicksHolder.UpdateMinTicks(firstEventTicksRowName, eventTimestamp.Ticks);
            var ttl = taskMeta.GetTtl();
            cfConnection.AddColumn(EventPointerFormatter.GetPartitionKey(eventTimestamp.Ticks), new Column
                {
                    Name = EventPointerFormatter.GetColumnName(eventTimestamp.Ticks, eventId),
                    Value = serializer.Serialize(new TaskMetaUpdatedEvent(taskMeta.Id, eventTimestamp.Ticks)),
                    Timestamp = eventTimestamp.Ticks,
                    TTL = ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int?)null,
                });
        }

        public long GetFirstEventTicks()
        {
            return minTicksHolder.GetMinTicks(firstEventTicksRowName);
        }

        [NotNull]
        public ( /*[NotNull]*/ TaskMetaUpdatedEvent Event, /*[NotNull]*/ string Offset)[] GetEvents([NotNull] string partitionKey,
                                                                                                    [NotNull] string exclusiveStartColumnName,
                                                                                                    [NotNull] string inclusiveEndColumnName,
                                                                                                    int eventsToFetch,
                                                                                                    out bool currentPartitionIsExhausted)
        {
            var columnsToFetch = eventsToFetch + 1;
            var columnsIncludingStartColumn = cfConnection.GetColumns(partitionKey, exclusiveStartColumnName, inclusiveEndColumnName, columnsToFetch, reversed : false);
            var events = columnsIncludingStartColumn.SkipWhile(x => x.Name == exclusiveStartColumnName)
                                                    .Select(x => (serializer.Deserialize<TaskMetaUpdatedEvent>(x.Value), Offset : x.Name))
                                                    .Take(eventsToFetch)
                                                    .ToArray();
            currentPartitionIsExhausted = columnsIncludingStartColumn.Length < columnsToFetch;
            return events;
        }

        public const string ColumnFamilyName = "RemoteTaskQueueEventLog";

        private const string firstEventTicksRowName = "firstEventTicksRowName";
        private readonly ISerializer serializer;
        private readonly IMinTicksHolder minTicksHolder;
        private readonly IColumnFamilyConnection cfConnection;
    }
}