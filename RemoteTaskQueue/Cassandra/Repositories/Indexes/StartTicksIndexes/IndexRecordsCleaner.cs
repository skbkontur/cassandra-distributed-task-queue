using System;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.EventIndexes;
using RemoteQueue.LocalTasks.TaskQueue;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

using log4net;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class IndexRecordsCleaner : ColumnFamilyRepositoryBase, IIndexRecordsCleaner
    {
        public IndexRecordsCleaner(
            IColumnFamilyRepositoryParameters parameters,
            ITaskMetaEventColumnInfoIndex taskMetaEventColumnInfoIndex,
            ISerializer serializer,
            IGlobalTime globalTime)
            : base(parameters, columnFamilyName)
        {
            this.taskMetaEventColumnInfoIndex = taskMetaEventColumnInfoIndex;
            taskQueue = new TaskQueue();
            this.serializer = serializer;
            this.globalTime = globalTime;
        }

        public void RemoveMeta(TaskMetaInformation obj)
        {
            RemoveIndexRecords(obj, TicksNameHelper.GetColumnInfo(obj.State, obj.MinimalStartTicks, ""));
        }

        public void RemoveIndexRecords(TaskMetaInformation obj, ColumnInfo columnInfo)
        {
            taskQueue.QueueTask(
                new ActionTask(
                    () =>
                        {
                            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
                            ColumnInfo[] columnInfos = taskMetaEventColumnInfoIndex.GetPreviousTaskEvents(obj.Id, columnInfo);
                            if(logger.IsDebugEnabled)
                                logger.Debug("Prepare for deleting\n" + string.Join("\n ", columnInfos.Select(GetColumnInfoLogString).ToArray()));
                            foreach(ColumnInfo info in columnInfos)
                                connection.DeleteBatch(info.RowKey, new[] {info.ColumnName}, globalTime.GetNowTicks());
                            taskMetaEventColumnInfoIndex.DeleteAllPrevious(obj.Id, columnInfo);
                        },
                    Guid.NewGuid().ToString()));
        }

        private string GetColumnInfoLogString(ColumnInfo columnInfo)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            Column col;
            string id = "Empty";
            if(connection.TryGetColumn(columnInfo.RowKey, columnInfo.ColumnName, out col))
                id = serializer.Deserialize<string>(col.Value);
            return columnInfo.RowKey + ", " + columnInfo.ColumnName + "," + id;
        }

        private readonly ILog logger = LogManager.GetLogger("TaskMinimalStartTicksIndex");
        private readonly TaskQueue taskQueue;
        private readonly ITaskMetaEventColumnInfoIndex taskMetaEventColumnInfoIndex;
        private readonly IGlobalTime globalTime;
        private readonly ISerializer serializer;
        private const string columnFamilyName = "TaskMinimalStartTicksIndex";
    }
}