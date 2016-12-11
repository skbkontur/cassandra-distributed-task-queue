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
        public GraphiteRemoteTaskQueueProfiler([NotNull] IProjectWideGraphitePathPrefixProvider graphitePathPrefixProvider, [NotNull] ICatalogueStatsDClient statsDClient)
        {
            if(string.IsNullOrWhiteSpace(graphitePathPrefixProvider.ProjectWideGraphitePathPrefix))
                this.statsDClient = EmptyStatsDClient.Instance;
            else
            {
                var keyNamePrefix = string.Format("{0}.SubSystem.RemoteTaskQueueTasks", graphitePathPrefixProvider.ProjectWideGraphitePathPrefix);
                this.statsDClient = statsDClient.WithScopes(new[]
                    {
                        string.Format("{0}.{1}", keyNamePrefix, Environment.MachineName),
                        string.Format("{0}.{1}", keyNamePrefix, "Total")
                    });
            }
        }

        public void ProcessTaskCreation([NotNull] TaskMetaInformation meta)
        {
            statsDClient.Increment("TasksQueued." + meta.Name);
        }

        public void ProcessTaskExecutionFinished([NotNull] TaskMetaInformation meta, [NotNull] HandleResult handleResult, TimeSpan taskExecutionTime)
        {
            statsDClient.Timing("ExecutionTime." + meta.Name, (long)taskExecutionTime.TotalMilliseconds);
            statsDClient.Increment("TasksExecuted." + meta.Name + "." + handleResult.FinishAction);
        }

        public void ProcessTaskExecutionFailed([NotNull] TaskMetaInformation meta, TimeSpan taskExecutionTime)
        {
            statsDClient.Timing("ExecutionTime." + meta.Name, (long)taskExecutionTime.TotalMilliseconds);
            statsDClient.Increment("TasksExecutionFailed." + meta.Name);
        }

        private readonly ICatalogueStatsDClient statsDClient;
    }
}