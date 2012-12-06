using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.Indexes.EventIndexes
{
    public class TaskMetaEventColumnInfoIndex : ColumnFamilyRepositoryBase, ITaskMetaEventColumnInfoIndex
    {
        public TaskMetaEventColumnInfoIndex(ISerializer serializer, IGlobalTime globalTime, IColumnFamilyRepositoryParameters parameters)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
        }

        public void AddTaskEventInfo(string taskId, ColumnInfo columnInfo)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            long ticks = TicksNameHelper.GetTicksFromColumnName(columnInfo.ColumnName);
            connection.AddColumn(taskId, new Column
                {
                    Name = TicksNameHelper.GetColumnName(ticks, Guid.NewGuid().ToString()),
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(columnInfo)
                });
        }

        public ColumnInfo[] GetPreviousTaskEvents(string taskId, ColumnInfo columnInfo)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            ColumnInfo[] columnInfos = connection.GetRow(taskId).Select(GetColumnInfo).ToArray();
            return columnInfos.Where(x => Less(x, columnInfo)).ToArray();
        }

        public void DeleteAllPrevious(string taskId, ColumnInfo columnInfo)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            Column[] columns = connection.GetRow(taskId).ToArray();
            IEnumerable<string> columnNames = columns.Where(x => Less(GetColumnInfo(x), columnInfo)).Select(x => x.Name);
            connection.DeleteBatch(taskId, columnNames);
        }

        public const string columnFamilyName = "TaskMetaEventColumnInfoIndex";

        private ColumnInfo GetColumnInfo(Column x)
        {
            return serializer.Deserialize<ColumnInfo>(x.Value);
        }

        private static bool Less(ColumnInfo columnInfo1, ColumnInfo columnInfo2)
        {
            return TicksNameHelper.GetTicksFromColumnName(columnInfo1.ColumnName) < TicksNameHelper.GetTicksFromColumnName(columnInfo2.ColumnName);
        }

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
    }
}