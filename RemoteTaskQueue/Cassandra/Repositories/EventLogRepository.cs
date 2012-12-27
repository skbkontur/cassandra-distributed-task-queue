using System;
using System.Collections.Generic;
using System.Globalization;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;
using SKBKontur.Cassandra.CassandraClient.Helpers;

namespace RemoteQueue.Cassandra.Repositories
{
    public class EventLogRepository : ColumnFamilyRepositoryBase, IEventLogRepository
    {
        public EventLogRepository(ISerializer serializer, IGlobalTime globalTime, IColumnFamilyRepositoryParameters parameters)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
            this.cassandraSettings = parameters.Settings;
        }

        public void AddEvent(TaskMetaUpdatedEvent taskMetaUpdatedEventEntity)
        {
            var ticks = globalTime.GetNowTicks();
            AddEventInternal(taskMetaUpdatedEventEntity, ticks);
        }

        public IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, int batchSize = 2000)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            long firstEventTicks;
            if(!TryGetFirstEventTicks(connection, out firstEventTicks))
                return new TaskMetaUpdatedEvent[0];
            firstEventTicks -= tickPartition; //note что это ?
            long diff = cassandraSettings.Attempts * TimeSpan.FromMilliseconds(cassandraSettings.Timeout).Ticks + TimeSpan.FromSeconds(10).Ticks;
            fromTicks = Math.Max(0, fromTicks - diff);
            if(fromTicks < firstEventTicks)
                fromTicks = firstEventTicks;
            return new GetEventLogEnumerable(serializer, connection, fromTicks, globalTime.GetNowTicks(), batchSize);
        }

        public const string columnFamilyName = "EventLog";

        private void AddEventInternal(TaskMetaUpdatedEvent taskMetaUpdatedEventEntity, long ticks)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            taskMetaUpdatedEventEntity.Ticks = ticks;
            UpdateFirstEventTicks(taskMetaUpdatedEventEntity.Ticks, connection);
            var columnInfo = GetColumnInfo(taskMetaUpdatedEventEntity.Ticks);
            connection.AddColumn(columnInfo.Item1, new Column
                {
                    Name = columnInfo.Item2,
                    Timestamp = taskMetaUpdatedEventEntity.Ticks,
                    Value = serializer.Serialize(taskMetaUpdatedEventEntity)
                });
        }

        private void UpdateFirstEventTicks(long ticks, IColumnFamilyConnection connection)
        {
            connection.AddColumn(firstEventTicksRow,
                                 new Column
                                     {
                                         Name = firstEventTicksColumnName,
                                         Value = StringHelpers.StringToBytes(ticks.ToString()),
                                         Timestamp = ticks
                                     });
        }

        private bool TryGetFirstEventTicks(IColumnFamilyConnection connection, out long ticks)
        {
            Column column;
            if(!connection.TryGetColumn(firstEventTicksRow, firstEventTicksColumnName, out column))
            {
                ticks = 0;
                return false;
            }
            ticks = long.Parse(StringHelpers.BytesToString(column.Value));
            return true;
        }

        private static Tuple<string, string> GetColumnInfo(long ticks)
        {
            var rowKey = (ticks / tickPartition).ToString();
            var columnName = ticks.ToString("D20", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid();
            return new Tuple<string, string>(rowKey, columnName);
        }

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly ICassandraSettings cassandraSettings;

        private static readonly string firstEventTicksRow = "firstEventTicks";
        private static readonly string firstEventTicksColumnName = "value";

        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
    }
}