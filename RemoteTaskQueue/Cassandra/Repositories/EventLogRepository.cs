using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using GroBuf;

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
            : base(cassandraCluster, settings, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
            this.ticksHolder = ticksHolder;
            var connectionParameters = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamilyName).GetConnectionParameters();
            UnstableZoneLength = TimeSpan.FromMilliseconds(connectionParameters.Attempts * connectionParameters.Timeout);
            eventLogTtl = TimeSpan.FromDays(30);
        }

        public void AddEvent(string taskId, long nowTicks)
        {
            if(string.IsNullOrEmpty(taskId))
                throw new Exception("Try to log event with null taskId!");
            var taskMetaUpdatedEventEntity = new TaskMetaUpdatedEvent
                {
                    TaskId = taskId,
                    Ticks = nowTicks
                };
            var connection = RetrieveColumnFamilyConnection();
            taskMetaUpdatedEventEntity.Ticks = nowTicks;
            ticksHolder.UpdateMinTicks(firstEventTicksRowName, nowTicks);
            var columnInfo = GetColumnInfo(nowTicks);
            connection.AddColumn(columnInfo.Item1, new Column
                {
                    Name = columnInfo.Item2,
                    Timestamp = nowTicks,
                    Value = serializer.Serialize(taskMetaUpdatedEventEntity),
                    TTL = (int) eventLogTtl.TotalSeconds
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

        [Obsolete("для конвертаций")]
        public void AddEvents(KeyValuePair<string, long>[] taskIdAndTicks)
        {
            if(taskIdAndTicks.Length == 0)
                return;

            var events = taskIdAndTicks.Select(kvp => new TaskMetaUpdatedEvent
                {
                    TaskId = kvp.Key,
                    Ticks = kvp.Value
                }).ToArray();
            var connection = RetrieveColumnFamilyConnection();
            ticksHolder.UpdateMinTicks(firstEventTicksRowName, events.Min(x => x.Ticks));

            var nowTicks = globalTime.GetNowTicks();
            var columns = events.Select(@event =>
                {
                    var columnInfo = GetColumnInfo(@event.Ticks);
                    return new KeyValuePair<string, Column>(columnInfo.Item1, new Column
                        {
                            Name = columnInfo.Item2,
                            Timestamp = nowTicks,
                            Value = serializer.Serialize(@event),
                            TTL = (int)eventLogTtl.TotalSeconds
                        });
                });
            connection.BatchInsert(columns.GroupBy(x => x.Key)
                                          .Select(group => new KeyValuePair<string, IEnumerable<Column>>(
                                                               group.Key,
                                                               group.Select(x => x.Value))));
        }

        public const string columnFamilyName = "RemoteTaskQueueEventLog";

        private static Tuple<string, string> GetColumnInfo(long ticks)
        {
            var rowKey = (ticks / tickPartition).ToString();
            var columnName = ticks.ToString("D20", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid();
            return new Tuple<string, string>(rowKey, columnName);
        }

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly ITicksHolder ticksHolder;

        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
        private TimeSpan eventLogTtl;
        private const string firstEventTicksRowName = "firstEventTicksRowName";
    }
}