using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Profiling
{
    internal class MetricsContext
    {
        private MetricsContext([NotNull] string contextName)
        {
            ContextName = contextName;
        }

        [NotNull]
        public string ContextName { get; }

        [NotNull]
        public MetricsContext SubContext([NotNull] string subContextName)
        {
            return new MetricsContext($"{ContextName}.{subContextName}");
        }

        [NotNull]
        public static MetricsContext For([NotNull] string contextName)
        {
            return new MetricsContext(contextName);
        }

        [NotNull]
        public static MetricsContext For([NotNull] TaskMetaInformation taskMeta)
        {
            return For("Tasks").SubContext(taskMeta.Name);
        }
    }
}