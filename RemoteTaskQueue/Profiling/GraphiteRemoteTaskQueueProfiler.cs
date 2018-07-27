using System;

using GroboContainer.Infection;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.Graphite.Client.Settings;
using SKBKontur.Catalogue.Core.Graphite.Client.StatsD;

namespace RemoteQueue.Profiling
{
    [IgnoredImplementation]
    public class GraphiteRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public GraphiteRemoteTaskQueueProfiler([NotNull] IGraphitePathPrefixProvider graphitePathPrefixProvider, [NotNull] ICatalogueStatsDClient statsDClient)
        {
            if (string.IsNullOrWhiteSpace(graphitePathPrefixProvider.GlobalPathPrefix))
                this.statsDClient = EmptyStatsDClient.Instance;
            else
            {
                var keyNamePrefix = $"{graphitePathPrefixProvider.GlobalPathPrefix}.SubSystem.RemoteTaskQueueTasks";
                this.statsDClient = statsDClient.WithScopes(new[]
                    {
                        $"{keyNamePrefix}.{Environment.MachineName}",
                        $"{keyNamePrefix}.Total"
                    });
            }
        }

        public void ProcessTaskCreation([NotNull] TaskMetaInformation meta)
        {
            statsDClient.Increment($"TasksQueued.{meta.Name}");
        }

        public void ProcessTaskExecutionFinished([NotNull] TaskMetaInformation meta, [NotNull] HandleResult handleResult, TimeSpan taskExecutionTime)
        {
            statsDClient.Timing($"ExecutionTime.{meta.Name}", (long)taskExecutionTime.TotalMilliseconds);
            statsDClient.Increment($"TasksExecuted.{meta.Name}.{handleResult.FinishAction}");
        }

        public void ProcessTaskExecutionFailed([NotNull] TaskMetaInformation meta, TimeSpan taskExecutionTime)
        {
            statsDClient.Timing($"ExecutionTime.{meta.Name}", (long)taskExecutionTime.TotalMilliseconds);
            statsDClient.Increment($"TasksExecutionFailed.{meta.Name}");
        }

        private readonly ICatalogueStatsDClient statsDClient;
    }
}