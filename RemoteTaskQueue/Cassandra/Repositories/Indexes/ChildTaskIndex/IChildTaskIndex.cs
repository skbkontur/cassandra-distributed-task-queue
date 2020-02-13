using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.ChildTaskIndex
{
    public interface IChildTaskIndex
    {
        void WriteIndexRecord([NotNull] TaskMetaInformation taskMeta, long timestamp);

        [NotNull]
        string[] GetChildTaskIds([NotNull] string taskId);
    }
}