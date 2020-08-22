using System;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.Graphite.Client;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Profiling
{
    public class GraphiteRtqProfiler : IRtqProfiler
    {
        public GraphiteRtqProfiler([NotNull] IStatsDClient statsDClient, [NotNull] IGraphiteClient graphiteClient, [NotNull] string statsDKeyNamePrefix, [NotNull] string consumerGraphitePathPrefix)
        {
            if (string.IsNullOrEmpty(statsDKeyNamePrefix))
                throw new InvalidOperationException("statsDKeyNamePrefix is empty");
            if (string.IsNullOrEmpty(consumerGraphitePathPrefix))
                throw new InvalidOperationException("consumerGraphitePathPrefix is empty");
            this.statsDClient = statsDClient.WithScopes($"{statsDKeyNamePrefix}.{Environment.MachineName}", $"{statsDKeyNamePrefix}.Total");
            this.graphiteClient = graphiteClient;
            this.consumerGraphitePathPrefix = FormatGraphitePathPrefix(consumerGraphitePathPrefix);
        }

        [NotNull]
        private static string FormatGraphitePathPrefix([NotNull] string graphitePathPrefix)
        {
            var processName = Process.GetCurrentProcess()
                                     .ProcessName
                                     .Replace(".exe", string.Empty)
                                     .Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries)
                                     .Last();
            return $"{graphitePathPrefix}.{Environment.MachineName}.{processName}";
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
            var graphitePath = $"{consumerGraphitePathPrefix}.LiveRecordTicksMarkerLag.{currentLiveRecordTicksMarker.TaskIndexShardKey.TaskState}.{currentLiveRecordTicksMarker.TaskIndexShardKey.TaskTopic}";
            graphiteClient.Send(graphitePath, (long)lag.TotalMilliseconds, nowTimestamp.ToDateTime());
        }

        private readonly IStatsDClient statsDClient;
        private readonly IGraphiteClient graphiteClient;
        private readonly string consumerGraphitePathPrefix;
    }
}