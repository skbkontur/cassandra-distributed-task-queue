using System;
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
        public TaskMinimalStartTicksIndex(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, IOldestLiveRecordTicksHolder oldestLiveRecordTicksHolder)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
            this.oldestLiveRecordTicksHolder = oldestLiveRecordTicksHolder;
        }

        [NotNull]
        public TaskColumnInfo IndexMeta([NotNull] TaskMetaInformation taskMeta)
        {
            oldestLiveRecordTicksHolder.MoveBackwardIfNecessary(taskMeta.State, taskMeta.MinimalStartTicks);
            var connection = RetrieveColumnFamilyConnection();
            var newColumnInfo = TicksNameHelper.GetColumnInfo(taskMeta);
            connection.AddColumn(newColumnInfo.RowKey, new Column
                {
                    Name = newColumnInfo.ColumnName,
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(taskMeta.Id)
                });
            return newColumnInfo;
        }

        public void UnindexMeta([NotNull] TaskColumnInfo taskColumnInfo)
        {
            var connection = RetrieveColumnFamilyConnection();
            connection.DeleteColumn(taskColumnInfo.RowKey, taskColumnInfo.ColumnName, (DateTime.UtcNow + TimeSpan.FromMinutes(1)).Ticks);
        }

        [NotNull]
        public IEnumerable<Tuple<string, TaskColumnInfo>> GetTaskIds(TaskState taskState, long toTicks, int batchSize)
        {
            var fromTicks = TryGetFromTicks(taskState);
            if(!fromTicks.HasValue)
                return new Tuple<string, TaskColumnInfo>[0];
            var connection = RetrieveColumnFamilyConnection();
            return new GetEventsEnumerable(taskState, serializer, connection, oldestLiveRecordTicksHolder, fromTicks.Value, toTicks, batchSize);
        }

        private long? TryGetFromTicks(TaskState taskState)
        {
            var oldestLiveRecordTicks = oldestLiveRecordTicksHolder.TryStartReadToEndSession(taskState);
            if(!oldestLiveRecordTicks.HasValue)
                return null;
            var overlapDuration = GetOverlapDuration(taskState);
            var fromTicks = oldestLiveRecordTicks.Value - overlapDuration.Ticks;
            var twoDaysSafetyBelt = (DateTime.UtcNow - TimeSpan.FromDays(2)).Ticks;
            return Math.Max(fromTicks, twoDaysSafetyBelt);
        }

        private TimeSpan GetOverlapDuration(TaskState taskState)
        {
            var utcNow = DateTime.UtcNow;
            DateTime lastBigOverlapMoment;
            if(!lastBigOverlapMomentsByTaskState.TryGetValue(taskState, out lastBigOverlapMoment) || utcNow - lastBigOverlapMoment > TimeSpan.FromMinutes(1))
            {
                lastBigOverlapMomentsByTaskState[taskState] = utcNow;
                //Сложно рассчитать математически правильный размер отката, и код постановки таски может измениться,
                //что потребует изменения этого отката. Поэтому берется, как кажется, с запасом
                return TimeSpan.FromMinutes(8); // Против адских затупов кассандры
            }
            return TimeSpan.FromMinutes(1); // Штатная зона нестабильности
        }

        public const string columnFamilyName = "TaskMinimalStartTicksIndex";

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly IOldestLiveRecordTicksHolder oldestLiveRecordTicksHolder;
        private readonly Dictionary<TaskState, DateTime> lastBigOverlapMomentsByTaskState = new Dictionary<TaskState, DateTime>();
    }
}