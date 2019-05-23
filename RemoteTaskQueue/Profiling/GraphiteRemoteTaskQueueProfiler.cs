using System;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;

using SkbKontur.Graphite.Client;

namespace RemoteQueue.Profiling
{
    public class GraphiteRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public GraphiteRemoteTaskQueueProfiler([NotNull] IStatsDClient statsDClient, [NotNull] IGraphiteClient graphiteClient)
        {
            const string keyNamePrefix = "SubSystem.RemoteTaskQueueTasks";
            this.statsDClient = statsDClient.WithScopes($"{keyNamePrefix}.{Environment.MachineName}", $"{keyNamePrefix}.Total");
            this.graphiteClient = graphiteClient;
            graphitePathPrefix = FormatGraphitePathPrefix();
        }

        [NotNull]
        private static string FormatGraphitePathPrefix()
        {
            var processName = Process.GetCurrentProcess()
                                     .ProcessName
                                     .Replace(".exe", string.Empty)
                                     .Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries)
                                     .Last();
            return $"SubSystem.RemoteTaskQueueMonitoring.{Environment.MachineName}.{processName}";
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

        public void ReportLiveRecordTicksMarkerLag([NotNull] Timestamp nowTimestamp, [NotNull] LiveRecordTicksMarkerState currentLiveRecordTicksMarker)
        {
            var lag = TimeSpan.FromTicks(nowTimestamp.Ticks - currentLiveRecordTicksMarker.CurrentTicks);
            var graphitePath = $"{graphitePathPrefix}.LiveRecordTicksMarkerLag.{currentLiveRecordTicksMarker.TaskIndexShardKey.TaskState}.{currentLiveRecordTicksMarker.TaskIndexShardKey.TaskTopic}";
            graphiteClient.Send(graphitePath, (long)lag.TotalMilliseconds, nowTimestamp.ToDateTime());
        }

        private readonly IStatsDClient statsDClient;
        private readonly IGraphiteClient graphiteClient;
        private readonly string graphitePathPrefix;
    }
}