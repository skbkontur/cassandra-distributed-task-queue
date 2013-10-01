using System;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class TaskMinimalStartTicksIndex : ColumnFamilyRepositoryBase, ITaskMinimalStartTicksIndex
    {
        public TaskMinimalStartTicksIndex(
            IColumnFamilyRepositoryParameters parameters,
            ITicksHolder ticksHolder,
            ISerializer serializer,
            IGlobalTime globalTime,
            ICassandraSettings cassandraSettings)
            : base(parameters, columnFamilyName)
        {
            this.cassandraSettings = cassandraSettings;
            this.ticksHolder = ticksHolder;
            this.serializer = serializer;
            this.globalTime = globalTime;
            minTicksCache = new MinTicksCache(this.ticksHolder);
        }

        public ColumnInfo IndexMeta(TaskMetaInformation taskMetaInformation)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            string state = taskMetaInformation.State.GetCassandraName();
            long ticks = taskMetaInformation.MinimalStartTicks;
            ticksHolder.UpdateMaxTicks(state, ticks);
            ticksHolder.UpdateMinTicks(state, ticks);

            ColumnInfo newColumnInfo = TicksNameHelper.GetColumnInfo(taskMetaInformation);
            connection.AddColumn(newColumnInfo.RowKey, new Column
                {
                    Name = newColumnInfo.ColumnName,
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(taskMetaInformation.Id)
                });

            var oldMetaIndex = taskMetaInformation.GetSnapshot();
            if(oldMetaIndex != null)
            {
                ColumnInfo oldColumnInfo = TicksNameHelper.GetColumnInfo(taskMetaInformation.GetSnapshot());
                if(!oldColumnInfo.Equals(newColumnInfo))
                    UnindexMeta(oldColumnInfo);
            }
            return newColumnInfo;
        }

        public void UnindexMeta(ColumnInfo columnInfo)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            connection.DeleteBatch(columnInfo.RowKey, new[] {columnInfo.ColumnName});
        }

        public IEnumerable<Tuple<string, ColumnInfo>> GetTaskIds(TaskState taskState, long nowTicks, int batchSize = 2000)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            long diff = cassandraSettings.Attempts * TimeSpan.FromMilliseconds(cassandraSettings.Timeout).Ticks + TimeSpan.FromSeconds(10).Ticks;
            long firstTicks;
            if(!TryGetFirstEventTicks(taskState, out firstTicks))
                return new Tuple<string, ColumnInfo>[0];
            firstTicks = (DateTime.UtcNow - TimeSpan.FromDays(1)).Ticks;
            return new GetEventsEnumerable(taskState, serializer, connection, minTicksCache, firstTicks, nowTicks, batchSize);
        }

        public const string columnFamilyName = "TaskMinimalStartTicksIndex";

        private bool TryGetFirstEventTicks(TaskState taskState, out long ticks)
        {
            ticks = minTicksCache.GetMinTicks(taskState);
            return ticks != 0;
        }

        private readonly ICassandraSettings cassandraSettings;
        private readonly ITicksHolder ticksHolder;
        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly IMinTicksCache minTicksCache;
    }
}