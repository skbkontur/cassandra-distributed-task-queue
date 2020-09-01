using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public interface IRtqTaskHandler
    {
        [NotNull]
        HandleResult HandleTask([NotNull] IRtqTaskProducer taskProducer, [NotNull] ISerializer serializer, [NotNull] Task task);
    }
}