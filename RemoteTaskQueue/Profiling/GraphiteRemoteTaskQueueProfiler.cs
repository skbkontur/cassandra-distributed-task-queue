using System;

using GroboContainer.Infection;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SkbKontur.Graphite.Client;

namespace RemoteQueue.Profiling
{
    [IgnoredImplementation]
    public class GraphiteRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public GraphiteRemoteTaskQueueProfiler([NotNull] IStatsDClient statsDClient)
        {
            const string keyNamePrefix = "SubSystem.RemoteTaskQueueTasks";
            this.statsDClient = statsDClient.WithScopes($"{keyNamePrefix}.{Environment.MachineName}", $"{keyNamePrefix}.Total");
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

        private readonly IStatsDClient statsDClient;
    }
}