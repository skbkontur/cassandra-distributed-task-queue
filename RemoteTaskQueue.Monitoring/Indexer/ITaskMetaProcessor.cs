using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public interface ITaskMetaProcessor
    {
        void IndexMetas([NotNull] TaskMetaInformation[] batch);
    }
}