using System;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.EventIndexes;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class TaskMinimalStartTicksIndex : ColumnFamilyRepositoryBase, ITaskMinimalStartTicksIndex
    {
        public TaskMinimalStartTicksIndex(
            IColumnFamilyRepositoryParameters parameters,
            ITaskMetaEventColumnInfoIndex taskMetaEventColumnInfoIndex,
            IIndexRecordsCleaner indexRecordsCleaner,
            ITicksHolder ticksHolder,
            ISerializer serializer,
            IGlobalTime globalTime,
            ICassandraSettings cassandraSettings)
            : base(parameters, columnFamilyName)
        {
            this.cassandraSettings = cassandraSettings;
            this.taskMetaEventColumnInfoIndex = taskMetaEventColumnInfoIndex;
            this.indexRecordsCleaner = indexRecordsCleaner;
            this.ticksHolder = ticksHolder;
            this.serializer = serializer;
            this.globalTime = globalTime;
            minTicksCache = new MinTicksCache(this.ticksHolder);
        }

        public void IndexMeta(TaskMetaInformation obj)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            string state = obj.State.GetCassandraName();
            long ticks = obj.MinimalStartTicks;
            ticksHolder.UpdateMaxTicks(state, ticks);
            ticksHolder.UpdateMinTicks(state, ticks);

            ColumnInfo columnInfo = TicksNameHelper.GetColumnInfo(obj.State, ticks, Guid.NewGuid().ToString());
            taskMetaEventColumnInfoIndex.AddTaskEventInfo(obj.Id, columnInfo);
            connection.AddColumn(columnInfo.RowKey, new Column
                {
                    Name = columnInfo.ColumnName,
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(obj.Id)
                });

            indexRecordsCleaner.RemoveIndexRecords(obj, columnInfo);
        }

        public IEnumerable<Tuple<string, ColumnInfo>> GetTaskIds(TaskState taskState, long nowTicks, int batchSize = 2000)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            //todo читать не с начала, отступать на diff
            long diff = cassandraSettings.Attempts * TimeSpan.FromMilliseconds(cassandraSettings.Timeout).Ticks + TimeSpan.FromSeconds(10).Ticks;
            long firstTicks;
            if(!TryGetFirstEventTicks(taskState, out firstTicks))
                return new Tuple<string, ColumnInfo>[0];
            firstTicks = Math.Max(0, firstTicks - diff);
            return new GetEventsEnumerable(taskState, serializer, connection, minTicksCache, firstTicks, nowTicks, batchSize);
        }

        public const string columnFamilyName = "TaskMinimalStartTicksIndex";

        private bool TryGetFirstEventTicks(TaskState taskState, out long ticks)
        {
            ticks = minTicksCache.GetMinTicks(taskState);
            return ticks != 0;
        }

        private readonly IIndexRecordsCleaner indexRecordsCleaner;

        private readonly ICassandraSettings cassandraSettings;
        private readonly ITaskMetaEventColumnInfoIndex taskMetaEventColumnInfoIndex;
        private readonly ITicksHolder ticksHolder;
        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly IMinTicksCache minTicksCache;
    }
}