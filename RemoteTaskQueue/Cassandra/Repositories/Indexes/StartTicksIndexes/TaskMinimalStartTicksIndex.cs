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
        public TaskMinimalStartTicksIndex(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, IFromTicksProvider fromTicksProvider)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
            this.fromTicksProvider = fromTicksProvider;
        }

        [NotNull]
        public ColumnInfo IndexMeta([NotNull] TaskMetaInformation taskMetaInformation)
        {
            fromTicksProvider.HandleTaskStateChange(taskMetaInformation);
            var connection = RetrieveColumnFamilyConnection();
            var newColumnInfo = TicksNameHelper.GetColumnInfo(taskMetaInformation);
            connection.AddColumn(newColumnInfo.RowKey, new Column
                {
                    Name = newColumnInfo.ColumnName,
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(taskMetaInformation.Id)
                });
            return newColumnInfo;
        }

        public void UnindexMeta([NotNull] ColumnInfo columnInfo)
        {
            var connection = RetrieveColumnFamilyConnection();
            connection.DeleteColumn(columnInfo.RowKey, columnInfo.ColumnName, (DateTime.UtcNow + TimeSpan.FromMinutes(1)).Ticks);
        }

        [NotNull]
        public IEnumerable<Tuple<string, ColumnInfo>> GetTaskIds(TaskState taskState, long toTicks, int batchSize)
        {
            var fromTicks = fromTicksProvider.TryGetFromTicks(taskState);
            if(!fromTicks.HasValue)
                return new Tuple<string, ColumnInfo>[0];
            var connection = RetrieveColumnFamilyConnection();
            return new GetEventsEnumerable(taskState, serializer, connection, fromTicksProvider, fromTicks.Value, toTicks, batchSize);
        }

        public const string columnFamilyName = "TaskMinimalStartTicksIndex";

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly IFromTicksProvider fromTicksProvider;
    }
}