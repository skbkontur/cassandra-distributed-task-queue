using System;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

using log4net;

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
            //Сложно рассчитать математически правильный размер отката, и код постановки таски может измениться,
            //что потребует изменения этого отката. Поэтому берется, как кажется, с запасом
            long diff = TimeSpan.FromMinutes(8).Ticks;
            long firstTicks;
            if(!TryGetFirstEventTicks(taskState, out firstTicks))
                return new Tuple<string, ColumnInfo>[0];
            var twoDaysEarlier = (DateTime.UtcNow - TimeSpan.FromDays(2)).Ticks;
            var firstTicksWithDiff = firstTicks - diff;
            var startTicks = Math.Max(twoDaysEarlier, firstTicksWithDiff);
            if(startTicks < (DateTime.UtcNow - TimeSpan.FromMinutes(12)).Ticks)
            {
                logger.WarnFormat("Strange startTicks. State = {4}: {0}. TwoDaysEarlier: {1}, FirsTicksWithDiff: {2}, FirstTicks: {3}", 
                    new DateTime(startTicks, DateTimeKind.Utc), 
                    new DateTime(twoDaysEarlier, DateTimeKind.Utc), 
                    new DateTime(firstTicksWithDiff, DateTimeKind.Utc),
                    new DateTime(firstTicks, DateTimeKind.Utc),
                    taskState);
            }
            return new GetEventsEnumerable(taskState, serializer, connection, minTicksCache, startTicks, nowTicks, batchSize);
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
        private readonly ILog logger = LogManager.GetLogger(typeof(TaskMinimalStartTicksIndex));
    }
}