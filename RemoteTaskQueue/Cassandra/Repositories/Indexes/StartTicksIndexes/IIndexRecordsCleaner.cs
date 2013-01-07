using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public interface IIndexRecordsCleaner
    {
        void RemoveMeta(TaskMetaInformation obj, ColumnInfo eventColumnInfo);
        void RemoveIndexRecords(TaskMetaInformation obj, ColumnInfo columnInfo);
    }
}