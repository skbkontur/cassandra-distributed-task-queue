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
                    Value = serializer.Serialize(taskMetaUpdatedEventEntity)
                });
        }

        public IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks, int batchSize = 2000)
        {
            var connection = RetrieveColumnFamilyConnection();
            var firstEventTicks = ticksHolder.GetMinTicks(firstEventTicksRowName);
            if(firstEventTicks == 0)
                return new TaskMetaUpdatedEvent[0];
            firstEventTicks -= tickPartition; //note что это ?
            var diff = cassandraSettings.Attempts * TimeSpan.FromMilliseconds(cassandraSettings.Timeout).Ticks + TimeSpan.FromSeconds(10).Ticks;
            fromTicks = new[] {0, fromTicks - diff, firstEventTicks}.Max();
            return new GetEventLogEnumerable(serializer, connection, fromTicks, globalTime.GetNowTicks(), batchSize);
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
        private readonly ICassandraSettings cassandraSettings;

        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
        private const string firstEventTicksRowName = "firstEventTicksRowName";
    }
}