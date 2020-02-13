using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
{
    public interface IRtqTaskHandlerRegistry
    {
        [NotNull]
        string[] GetAllTaskTopicsToHandle();

        bool ContainsHandlerFor([NotNull] string taskName);

        [NotNull]
        IRtqTaskHandler CreateHandlerFor([NotNull] string taskName);
    }
}