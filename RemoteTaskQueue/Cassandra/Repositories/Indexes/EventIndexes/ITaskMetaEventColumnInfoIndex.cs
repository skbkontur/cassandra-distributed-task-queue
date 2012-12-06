namespace RemoteQueue.Cassandra.Repositories.Indexes.EventIndexes
{
    public interface ITaskMetaEventColumnInfoIndex
    {
        ColumnInfo[] GetPreviousTaskEvents(string taskId, ColumnInfo columnInfo);
        void AddTaskEventInfo(string taskId, ColumnInfo columnInfo);
        void DeleteAllPrevious(string taskId, ColumnInfo columnInfo);
    }
}