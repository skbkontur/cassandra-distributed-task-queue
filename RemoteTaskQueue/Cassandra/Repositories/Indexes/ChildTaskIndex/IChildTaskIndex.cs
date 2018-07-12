using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex
{
    public interface IChildTaskIndex
    {
        void WriteIndexRecord([NotNull] TaskMetaInformation taskMeta, long timestamp);

        [NotNull]
        string[] GetChildTaskIds([NotNull] string taskId);
    }
}