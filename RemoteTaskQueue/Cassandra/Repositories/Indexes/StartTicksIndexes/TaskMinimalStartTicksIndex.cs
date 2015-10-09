using System;
using System.Collections.Generic;

using GroBuf;

using JetBrains.Annotations;

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

        public void AddRecord([NotNull] TaskIndexRecord taskIndexRecord)
        {
            oldestLiveRecordTicksHolder.MoveBackwardIfNecessary(taskIndexRecord.TaskNameAndState, taskIndexRecord.MinimalStartTicks);
            var connection = RetrieveColumnFamilyConnection();
            var rowKey = TicksNameHelper.GetRowKey(taskIndexRecord.TaskNameAndState, taskIndexRecord.MinimalStartTicks);
            var columnName = TicksNameHelper.GetColumnName(taskIndexRecord.MinimalStartTicks, taskIndexRecord.TaskId);
            connection.AddColumn(rowKey, new Column
                {
                    Name = columnName,
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(taskIndexRecord.TaskId)
                });
        }

        public void RemoveRecord([NotNull] TaskIndexRecord taskIndexRecord)
        {
            var connection = RetrieveColumnFamilyConnection();
            var rowKey = TicksNameHelper.GetRowKey(taskIndexRecord.TaskNameAndState, taskIndexRecord.MinimalStartTicks);
            var columnName = TicksNameHelper.GetColumnName(taskIndexRecord.MinimalStartTicks, taskIndexRecord.TaskId);
            connection.DeleteColumn(rowKey, columnName, (DateTime.UtcNow + TimeSpan.FromMinutes(1)).Ticks);
        }

        [NotNull]
        public IEnumerable<TaskIndexRecord> GetRecords([NotNull] TaskNameAndState taskNameAndState, long toTicks, int batchSize)
        {
            var fromTicks = TryGetFromTicks(taskNameAndState);
            if(!fromTicks.HasValue)
                return new TaskIndexRecord[0];
            var connection = RetrieveColumnFamilyConnection();
            return new GetEventsEnumerable(taskNameAndState, serializer, connection, oldestLiveRecordTicksHolder, fromTicks.Value, toTicks, batchSize);
        }

        private long? TryGetFromTicks([NotNull] TaskNameAndState taskNameAndState)
        {
            var oldestLiveRecordTicks = oldestLiveRecordTicksHolder.TryStartReadToEndSession(taskNameAndState);
            if(!oldestLiveRecordTicks.HasValue)
                return null;
            var overlapDuration = GetOverlapDuration(taskNameAndState);
            var fromTicks = oldestLiveRecordTicks.Value - overlapDuration.Ticks;
            var twoDaysSafetyBelt = (DateTime.UtcNow - TimeSpan.FromDays(2)).Ticks;
            return Math.Max(fromTicks, twoDaysSafetyBelt);
        }

        private TimeSpan GetOverlapDuration([NotNull] TaskNameAndState taskNameAndState)
        {
            var utcNow = DateTime.UtcNow;
            DateTime lastBigOverlapMoment;
            if(!lastBigOverlapMomentsByTaskState.TryGetValue(taskNameAndState, out lastBigOverlapMoment) || utcNow - lastBigOverlapMoment > TimeSpan.FromMinutes(1))
            {
                lastBigOverlapMomentsByTaskState[taskNameAndState] = utcNow;
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
        private readonly Dictionary<TaskNameAndState, DateTime> lastBigOverlapMomentsByTaskState = new Dictionary<TaskNameAndState, DateTime>();
    }
}