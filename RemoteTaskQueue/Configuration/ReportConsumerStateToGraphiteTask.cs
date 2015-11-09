using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Core.Graphite.Client.Settings;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteQueue.Configuration
{
    public class ReportConsumerStateToGraphiteTask : PeriodicTaskBase
    {
        public ReportConsumerStateToGraphiteTask(ICatalogueGraphiteClient graphiteClient, IProjectWideGraphitePathPrefixProvider graphitePathPrefixProvider, List<IHandlerManager> handlerManagers)
        {
            this.graphiteClient = graphiteClient;
            this.handlerManagers = handlerManagers;
            graphitePathPrefix = FormatGraphitePathPrefix(graphitePathPrefixProvider.ProjectWideGraphitePathPrefix);
        }

        [NotNull]
        private static string FormatGraphitePathPrefix([NotNull] string projectWideGraphitePathPrefix)
        {
            var processName = Process.GetCurrentProcess()
                                     .ProcessName
                                     .Replace(".exe", string.Empty)
                                     .Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries)
                                     .Last();
            return string.Format("{0}.SubSystem.RemoteTaskQueueMonitoring.{1}.{2}", projectWideGraphitePathPrefix, Environment.MachineName, processName);
        }

        public override sealed void Run()
        {
            var now = Timestamp.Now;
            foreach(var handlerManager in handlerManagers)
            {
                foreach(var marker in handlerManager.GetCurrentLiveRecordTicksMarkers())
                {
                    var lag = TimeSpan.FromTicks(now.Ticks - marker.CurrentTicks);
                    var graphitePath = string.Format("{0}.LiveRecordTicksMarkerLag.{1}.{2}", graphitePathPrefix, marker.TaskIndexShardKey.TaskState, marker.TaskIndexShardKey.TaskTopic);
                    graphiteClient.Send(graphitePath, (long)lag.TotalMilliseconds, now.ToDateTime());
                }
            }
        }

        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly List<IHandlerManager> handlerManagers;
        private readonly string graphitePathPrefix;
    }
}