using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public interface IWriteIndexNameFactory
    {
        [NotNull]
        string GetIndexForTask([NotNull] TaskMetaInformation taskMetaInformation);
    }
}