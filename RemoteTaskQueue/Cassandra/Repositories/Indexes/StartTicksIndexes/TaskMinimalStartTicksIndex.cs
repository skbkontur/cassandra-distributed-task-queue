using System;
using System.Collections.Generic;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.Objects;

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

        [CanBeNull]
        public LiveRecordTicksMarkerState TryGetCurrentLiveRecordTicksMarker([NotNull] TaskIndexShardKey taskIndexShardKey)
        {
            return oldestLiveRecordTicksHolder.TryGetCurrentMarkerValue(taskIndexShardKey).With(x => x.State);
        }

        public void AddRecord([NotNull] TaskIndexRecord taskIndexRecord)
        {
            oldestLiveRecordTicksHolder.MoveMarkerBackwardIfNecessary(taskIndexRecord.TaskIndexShardKey, taskIndexRecord.MinimalStartTicks);
            var connection = RetrieveColumnFamilyConnection();
            var rowKey = CassandraNameHelper.GetRowKey(taskIndexRecord.TaskIndexShardKey, taskIndexRecord.MinimalStartTicks);
            var columnName = CassandraNameHelper.GetColumnName(taskIndexRecord.MinimalStartTicks, taskIndexRecord.TaskId);
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
            var rowKey = CassandraNameHelper.GetRowKey(taskIndexRecord.TaskIndexShardKey, taskIndexRecord.MinimalStartTicks);
            var columnName = CassandraNameHelper.GetColumnName(taskIndexRecord.MinimalStartTicks, taskIndexRecord.TaskId);
            connection.DeleteColumn(rowKey, columnName, (DateTime.UtcNow + TimeSpan.FromMinutes(1)).Ticks);
        }

        [NotNull]
        public IEnumerable<TaskIndexRecord> GetRecords([NotNull] TaskIndexShardKey taskIndexShardKey, long toTicks, int batchSize)
        {
            ILiveRecordTicksMarker liveRecordTicksMarker;
            var fromTicks = TryGetFromTicks(taskIndexShardKey, out liveRecordTicksMarker);
            if(!fromTicks.HasValue)
                return new TaskIndexRecord[0];
            var connection = RetrieveColumnFamilyConnection();
            return new GetEventsEnumerable(liveRecordTicksMarker, serializer, connection, fromTicks.Value, toTicks, batchSize);
        }

        private long? TryGetFromTicks([NotNull] TaskIndexShardKey taskIndexShardKey, out ILiveRecordTicksMarker liveRecordTicksMarker)
        {
            liveRecordTicksMarker = oldestLiveRecordTicksHolder.TryGetCurrentMarkerValue(taskIndexShardKey);
            if(liveRecordTicksMarker == null)
                return null;
            var overlapDuration = GetOverlapDuration(taskIndexShardKey);
            var fromTicks = liveRecordTicksMarker.State.CurrentTicks - overlapDuration.Ticks;
            var twoDaysSafetyBelt = (DateTime.UtcNow - TimeSpan.FromDays(2)).Ticks;
            return Math.Max(fromTicks, twoDaysSafetyBelt);
        }

        private TimeSpan GetOverlapDuration([NotNull] TaskIndexShardKey taskIndexShardKey)
        {
            var utcNow = DateTime.UtcNow;
            DateTime lastBigOverlapMoment;
            if(!lastBigOverlapMomentsByShardKey.TryGetValue(taskIndexShardKey, out lastBigOverlapMoment) || utcNow - lastBigOverlapMoment > TimeSpan.FromMinutes(1))
            {
                lastBigOverlapMomentsByShardKey[taskIndexShardKey] = utcNow;
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
        private readonly Dictionary<TaskIndexShardKey, DateTime> lastBigOverlapMomentsByShardKey = new Dictionary<TaskIndexShardKey, DateTime>();
    }
}