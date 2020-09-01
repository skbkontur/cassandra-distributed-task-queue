using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    public interface IRemoteTask
    {
        [NotNull]
        string Id { get; }

        [NotNull]
        string Queue();

        [NotNull]
        string Queue(TimeSpan delay);

        string ParentTaskId { get; }
    }
}