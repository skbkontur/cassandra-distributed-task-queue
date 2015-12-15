using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex
{
    public interface IChildTaskIndex
    {
        void AddMeta([NotNull] TaskMetaInformation meta);

        [NotNull]
        string[] GetChildTaskIds([NotNull] string taskId);
    }
}