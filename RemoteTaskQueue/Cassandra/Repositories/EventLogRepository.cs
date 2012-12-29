using System;
using System.Collections.Generic;
using System.Globalization;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Cassandra.Repositories
{
    public class EventLogRepository : ColumnFamilyRepositoryBase, IEventLogRepository
    {
        public EventLogRepository(ISerializer serializer, IGlobalTime globalTime, IColumnFamilyRepositoryParameters parameters, ITicksHolder ticksHolder)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
            this.ticksHolder = ticksHolder;
            cassandraSettings = parameters.Settings;
        }

        public void AddEvent(TaskMetaUpdatedEvent taskMetaUpdatedEventEntity)
        {
            var ticks = globalTime.GetNowTicks();
            AddEventInternal(taskMetaUpdatedEventEntity, ticks);
        }

        public IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, int batchSize = 2000)
        {
            var connection = RetrieveColumnFamilyConnection();
            var firstEventTicks = ticksHolder.GetMinTicks(firstEventTicksRowName);
            if(firstEventTicks == 0)
                return new TaskMetaUpdatedEvent[0];
            firstEventTicks -= tickPartition; //note что это ?
            var diff = cassandraSettings.Attempts * TimeSpan.FromMilliseconds(cassandraSettings.Timeout).Ticks + TimeSpan.FromSeconds(10).Ticks;
            fromTicks = Math.Max(0, fromTicks - diff);
            if(fromTicks < firstEventTicks)
                fromTicks = firstEventTicks;
            return new GetEventLogEnumerable(serializer, connection, fromTicks, globalTime.GetNowTicks(), batchSize);
        }

        public const string columnFamilyName = "RemoteTaskQueueEventLog";

        private void AddEventInternal(TaskMetaUpdatedEvent taskMetaUpdatedEventEntity, long ticks)
        {
            var connection = RetrieveColumnFamilyConnection();
            taskMetaUpdatedEventEntity.Ticks = ticks;
            ticksHolder.UpdateMinTicks(firstEventTicksRowName, taskMetaUpdatedEventEntity.Ticks);
            var columnInfo = GetColumnInfo(taskMetaUpdatedEventEntity.Ticks);
            connection.AddColumn(columnInfo.Item1, new Column
                {
                    Name = columnInfo.Item2,
                    Timestamp = taskMetaUpdatedEventEntity.Ticks,
                    Value = serializer.Serialize(taskMetaUpdatedEventEntity)
                });
        }

        private static Tuple<string, string> GetColumnInfo(long ticks)
        {
            var rowKey = (ticks / tickPartition).ToString();
            var columnName = ticks.ToString("D20", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid();
            return new Tuple<string, string>(rowKey, columnName);
        }

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly ITicksHolder ticksHolder;
        private readonly ICassandraSettings cassandraSettings;

        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
        private const string firstEventTicksRowName = "firstEventTicksRowName";
    }
}