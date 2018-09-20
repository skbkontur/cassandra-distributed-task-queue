using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Handling;

using SkbKontur.Graphite.Client;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Graphite;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteQueue.Configuration
{
    public class ReportConsumerStateToGraphiteTask : PeriodicTaskBase
    {
        public ReportConsumerStateToGraphiteTask(IGraphiteClient graphiteClient, IGraphitePathPrefixProvider graphitePathPrefixProvider, List<IHandlerManager> handlerManagers)
        {
            this.graphiteClient = graphiteClient;
            this.handlerManagers = handlerManagers;
            graphitePathPrefix = FormatGraphitePathPrefix(graphitePathPrefixProvider.GlobalPathPrefix);
            startupTimestamp = Timestamp.Now;
        }

        [NotNull]
        private static string FormatGraphitePathPrefix([NotNull] string projectWideGraphitePathPrefix)
        {
            var processName = Process.GetCurrentProcess()
                                     .ProcessName
                                     .Replace(".exe", string.Empty)
                                     .Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries)
                                     .Last();
            return $"{projectWideGraphitePathPrefix}.SubSystem.RemoteTaskQueueMonitoring.{Environment.MachineName}.{processName}";
        }

        public override sealed void Run()
        {
            var now = Timestamp.Now;
            if (now - startupTimestamp < TimeSpan.FromMinutes(3))
                return;
            foreach (var handlerManager in handlerManagers)
            {
                foreach (var marker in handlerManager.GetCurrentLiveRecordTicksMarkers())
                {
                    var lag = TimeSpan.FromTicks(now.Ticks - marker.CurrentTicks);
                    var graphitePath = $"{graphitePathPrefix}.LiveRecordTicksMarkerLag.{marker.TaskIndexShardKey.TaskState}.{marker.TaskIndexShardKey.TaskTopic}";
                    graphiteClient.Send(graphitePath, (long)lag.TotalMilliseconds, now.ToDateTime());
                }
            }
        }

        private readonly IGraphiteClient graphiteClient;
        private readonly List<IHandlerManager> handlerManagers;
        private readonly string graphitePathPrefix;
        private readonly Timestamp startupTimestamp;
    }
}