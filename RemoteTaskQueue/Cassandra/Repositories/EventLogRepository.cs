using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Cassandra.Repositories
{
    public class EventLogRepository : ColumnFamilyRepositoryBase, IEventLogRepository
    {
        public EventLogRepository(ISerializer serializer, IGlobalTime globalTime, ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings settings, ITicksHolder ticksHolder)
            : base(cassandraCluster, settings, ColumnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
            this.ticksHolder = ticksHolder;
            var connectionParameters = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, ColumnFamilyName).GetConnectionParameters();
            UnstableZoneLength = TimeSpan.FromMilliseconds(connectionParameters.Attempts * connectionParameters.Timeout);
        }

        public void AddEvent([NotNull] TaskMetaInformation taskMeta, long nowTicks)
        {
            ticksHolder.UpdateMinTicks(firstEventTicksRowName, nowTicks);
            var columnInfo = GetColumnInfo(nowTicks);
            var ttl = taskMeta.GetTtl();
            RetrieveColumnFamilyConnection().AddColumn(columnInfo.Item1, new Column
                {
                    Name = columnInfo.Item2,
                    Timestamp = nowTicks,
                    Value = serializer.Serialize(new TaskMetaUpdatedEvent
                        {
                            TaskId = taskMeta.Id,
                            Ticks = nowTicks,
                        }),
                    TTL = ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int?)null,
                });
        }

        public IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, int batchSize = 2000)
        {
            var connection = RetrieveColumnFamilyConnection();
            var firstEventTicks = ticksHolder.GetMinTicks(firstEventTicksRowName);
            if(firstEventTicks == 0)
                return new TaskMetaUpdatedEvent[0];
            firstEventTicks -= tickPartition; //note что это ?
            fromTicks = new[] {0, fromTicks, firstEventTicks}.Max();
            return new GetEventLogEnumerable(serializer, connection, fromTicks, globalTime.GetNowTicks(), batchSize);
        }

        public TimeSpan UnstableZoneLength { get; private set; }

        [NotNull]
        private static Tuple<string, string> GetColumnInfo(long ticks)
        {
            var rowKey = (ticks / tickPartition).ToString();
            var columnName = ticks.ToString("D20", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid();
            return new Tuple<string, string>(rowKey, columnName);
        }

        public const string ColumnFamilyName = "RemoteTaskQueueEventLog";
        private const string firstEventTicksRowName = "firstEventTicksRowName";

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly ITicksHolder ticksHolder;

        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}