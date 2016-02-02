using System;
using System.Collections.Generic;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class TaskMinimalStartTicksIndex : ColumnFamilyRepositoryBase, ITaskMinimalStartTicksIndex
    {
        public TaskMinimalStartTicksIndex(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IOldestLiveRecordTicksHolder oldestLiveRecordTicksHolder)
            : base(parameters, ColumnFamilyName)
        {
            this.serializer = serializer;
            this.oldestLiveRecordTicksHolder = oldestLiveRecordTicksHolder;
        }

        [CanBeNull]
        public LiveRecordTicksMarkerState TryGetCurrentLiveRecordTicksMarker([NotNull] TaskIndexShardKey taskIndexShardKey)
        {
            return oldestLiveRecordTicksHolder.TryGetCurrentMarkerValue(taskIndexShardKey).With(x => x.State);
        }

        public void AddRecord([NotNull] TaskIndexRecord taskIndexRecord, long timestamp)
        {
            oldestLiveRecordTicksHolder.MoveMarkerBackwardIfNecessary(taskIndexRecord.TaskIndexShardKey, taskIndexRecord.MinimalStartTicks);
            var connection = RetrieveColumnFamilyConnection();
            var rowKey = CassandraNameHelper.GetRowKey(taskIndexRecord.TaskIndexShardKey, taskIndexRecord.MinimalStartTicks);
            var columnName = CassandraNameHelper.GetColumnName(taskIndexRecord.MinimalStartTicks, taskIndexRecord.TaskId);
            connection.AddColumn(rowKey, new Column
                {
                    Name = columnName,
                    Timestamp = timestamp,
                    Value = serializer.Serialize(taskIndexRecord.TaskId),
                    TTL = null,
                });
        }

        public void RemoveRecord([NotNull] TaskIndexRecord taskIndexRecord, long timestamp)
        {
            var connection = RetrieveColumnFamilyConnection();
            var rowKey = CassandraNameHelper.GetRowKey(taskIndexRecord.TaskIndexShardKey, taskIndexRecord.MinimalStartTicks);
            var columnName = CassandraNameHelper.GetColumnName(taskIndexRecord.MinimalStartTicks, taskIndexRecord.TaskId);
            connection.DeleteColumn(rowKey, columnName, timestamp);
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
            var safetyBelt = (Timestamp.Now - TimeSpan.FromHours(6)).Ticks;
            if(fromTicks < safetyBelt)
            {
                Log.For(this).WarnFormat("fromTicks ({0}) < safetyBelt ({1})", new Timestamp(fromTicks), new Timestamp(safetyBelt));
                return safetyBelt;
            }
            return fromTicks;
        }

        private TimeSpan GetOverlapDuration([NotNull] TaskIndexShardKey taskIndexShardKey)
        {
            lock(locker)
            {
                var now = Timestamp.Now;
                Timestamp lastBigOverlapMoment;
                if(!lastBigOverlapMomentsByShardKey.TryGetValue(taskIndexShardKey, out lastBigOverlapMoment) || now - lastBigOverlapMoment > TimeSpan.FromMinutes(1))
                {
                    lastBigOverlapMomentsByShardKey[taskIndexShardKey] = now;
                    //Сложно рассчитать математически правильный размер отката, и код постановки таски может измениться,
                    //что потребует изменения этого отката. Поэтому берется, как кажется, с запасом
                    return TimeSpan.FromMinutes(8); // Против адских затупов кассандры
                }
                return TimeSpan.FromMinutes(1); // Штатная зона нестабильности
            }
        }

        public const string ColumnFamilyName = "TaskMinimalStartTicksIndex";

        private readonly ISerializer serializer;
        private readonly IOldestLiveRecordTicksHolder oldestLiveRecordTicksHolder;
        private readonly object locker = new object();
        private readonly Dictionary<TaskIndexShardKey, Timestamp> lastBigOverlapMomentsByShardKey = new Dictionary<TaskIndexShardKey, Timestamp>();
    }
}