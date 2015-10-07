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
        public TaskColumnInfo IndexMeta([NotNull] TaskMetaInformation taskMeta)
        {
            fromTicksProvider.HandleTaskStateChange(taskMeta);
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
            var fromTicks = fromTicksProvider.TryGetFromTicks(taskState);
            if(!fromTicks.HasValue)
                return new Tuple<string, TaskColumnInfo>[0];
            var connection = RetrieveColumnFamilyConnection();
            return new GetEventsEnumerable(taskState, serializer, connection, fromTicksProvider, fromTicks.Value, toTicks, batchSize);
        }

        public const string columnFamilyName = "TaskMinimalStartTicksIndex";

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly IFromTicksProvider fromTicksProvider;
    }
}