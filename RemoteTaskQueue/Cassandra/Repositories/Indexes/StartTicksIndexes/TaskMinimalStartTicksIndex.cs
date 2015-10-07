﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class TaskMinimalStartTicksIndex : ColumnFamilyRepositoryBase, ITaskMinimalStartTicksIndex
    {
        public TaskMinimalStartTicksIndex(
            IColumnFamilyRepositoryParameters parameters,
            ITicksHolder ticksHolder,
            ISerializer serializer,
            IGlobalTime globalTime)
            : base(parameters, columnFamilyName)
        {
            this.ticksHolder = ticksHolder;
            this.serializer = serializer;
            this.globalTime = globalTime;
            minTicksCache = new MinTicksCache(this.ticksHolder);
        }

        [NotNull]
        public ColumnInfo IndexMeta([NotNull] TaskMetaInformation taskMetaInformation)
        {
            var connection = RetrieveColumnFamilyConnection();
            var state = taskMetaInformation.State.GetCassandraName();
            var ticks = taskMetaInformation.MinimalStartTicks;
            ticksHolder.UpdateMinTicks(state, ticks);

            var newColumnInfo = TicksNameHelper.GetColumnInfo(taskMetaInformation);
            connection.AddColumn(newColumnInfo.RowKey, new Column
                {
                    Name = newColumnInfo.ColumnName,
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(taskMetaInformation.Id)
                });
            return newColumnInfo;
        }

        public void UnindexMeta(string taskId, ColumnInfo columnInfo)
        {
            var connection = RetrieveColumnFamilyConnection();
            connection.DeleteColumn(columnInfo.RowKey, columnInfo.ColumnName, (DateTime.UtcNow + TimeSpan.FromMinutes(1)).Ticks);
        }

        public IEnumerable<Tuple<string, ColumnInfo>> GetTaskIds(TaskState taskState, long toTicks, int batchSize)
        {
            var connection = RetrieveColumnFamilyConnection();
            var diff = GetDiff(taskState).Ticks;
            var firstTicks = minTicksCache.GetMinTicks(taskState);
            if(firstTicks == 0)
                return new Tuple<string, ColumnInfo>[0];
            var twoDaysEarlier = (DateTime.UtcNow - TimeSpan.FromDays(2)).Ticks;
            var firstTicksWithDiff = firstTicks - diff;
            var fromTicks = Math.Max(twoDaysEarlier, firstTicksWithDiff);
            var getEventsEnumerable = new GetEventsEnumerable(taskState, serializer, connection, minTicksCache, fromTicks, toTicks, batchSize);
            return getEventsEnumerable;
        }

        private TimeSpan GetDiff(TaskState taskState)
        {
            var lastBigDiffTime = lastBigDiffTimes.GetOrAdd(taskState, t => DateTime.MinValue);
            var now = DateTime.UtcNow;
            if(now - lastBigDiffTime > TimeSpan.FromMinutes(1))
            {
                lastBigDiffTimes.AddOrUpdate(taskState, DateTime.MinValue, (t, p) => now);
                //Сложно рассчитать математически правильный размер отката, и код постановки таски может измениться,
                //что потребует изменения этого отката. Поэтому берется, как кажется, с запасом
                return TimeSpan.FromMinutes(8); // Против адских затупов кассандры
            }
            return TimeSpan.FromMinutes(1); // Штатная зона нестабильности
        }

        public const string columnFamilyName = "TaskMinimalStartTicksIndex";

        private readonly ConcurrentDictionary<TaskState, DateTime> lastBigDiffTimes = new ConcurrentDictionary<TaskState, DateTime>();
        private readonly ITicksHolder ticksHolder;
        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly IMinTicksCache minTicksCache;
    }
}