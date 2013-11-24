using System;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

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
            inProcessTasksCache = new TasksCache();
        }

        public ColumnInfo IndexMeta(TaskMetaInformation taskMetaInformation)
        {
            var connection = RetrieveColumnFamilyConnection();
            var state = taskMetaInformation.State.GetCassandraName();
            var ticks = taskMetaInformation.MinimalStartTicks;
            ticksHolder.UpdateMaxTicks(state, ticks);
            ticksHolder.UpdateMinTicks(state, ticks);

            var newColumnInfo = TicksNameHelper.GetColumnInfo(taskMetaInformation);
            connection.AddColumn(newColumnInfo.RowKey, new Column
                {
                    Name = newColumnInfo.ColumnName,
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(taskMetaInformation.Id)
                });

            var oldMetaIndex = taskMetaInformation.GetSnapshot();
            if(oldMetaIndex != null)
            {
                var oldColumnInfo = TicksNameHelper.GetColumnInfo(taskMetaInformation.GetSnapshot());
                if(!oldColumnInfo.Equals(newColumnInfo))
                    UnindexMeta(taskMetaInformation.Id, oldColumnInfo);
            }
            return newColumnInfo;
        }

        public void UnindexMeta(string taskId, ColumnInfo columnInfo)
        {
            inProcessTasksCache.Remove(taskId);
            var connection = RetrieveColumnFamilyConnection();
            connection.DeleteBatch(columnInfo.RowKey, new[] {columnInfo.ColumnName});
        }

        public IEnumerable<Tuple<string, ColumnInfo>> GetTaskIds(TaskState taskState, long nowTicks, int batchSize = 2000)
        {
            var connection = RetrieveColumnFamilyConnection();
            //Сложно рассчитать математически правильный размер отката, и код постановки таски может измениться,
            //что потребует изменения этого отката. Поэтому берется, как кажется, с запасом
            var diff = TimeSpan.FromMinutes(8).Ticks;
            long firstTicks;
            if(!TryGetFirstEventTicks(taskState, out firstTicks))
                return new Tuple<string, ColumnInfo>[0];
            var twoDaysEarlier = (DateTime.UtcNow - TimeSpan.FromDays(2)).Ticks;
            var firstTicksWithDiff = firstTicks - diff;
            var startTicks = Math.Max(twoDaysEarlier, firstTicksWithDiff);
            var getEventsEnumerable = new GetEventsEnumerable(taskState, serializer, connection, minTicksCache, startTicks, nowTicks, batchSize);
            if(taskState == TaskState.InProcess)
                return inProcessTasksCache.PassThroughtCache(getEventsEnumerable);
            return getEventsEnumerable;
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
        private readonly TasksCache inProcessTasksCache;
    }
}