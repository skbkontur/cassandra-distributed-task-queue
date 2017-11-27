using JetBrains.Annotations;

namespace RemoteQueue.Profiling
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
    }
}