using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteQueue.Configuration
{
    public class ReportConsumerStateToGraphiteTask : PeriodicTaskBase
    {
        public ReportConsumerStateToGraphiteTask(ICatalogueGraphiteClient graphiteClient, List<IHandlerManager> handlerManagers)
        {
            this.graphiteClient = graphiteClient;
            this.handlerManagers = handlerManagers;
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
            return string.Format("EDI.SubSystem.RemoteTaskQueueMonitoring.{0}.{1}", Environment.MachineName, processName);
        }

        public override sealed void Run()
        {
            var now = Timestamp.Now;
            foreach(var handlerManager in handlerManagers)
            {
                foreach(var marker in handlerManager.GetCurrentLiveRecordTicksMarkers())
                {
                    var lag = TimeSpan.FromTicks(now.Ticks - marker.CurrentTicks);
                    var graphitePath = string.Format("{0}.LiveRecordTicksMarkerLag.{1}.{2}", graphitePathPrefix, marker.TaskTopicAndState.TaskState, marker.TaskTopicAndState.TaskTopic);
                    graphiteClient.Send(graphitePath, (long)lag.TotalMilliseconds, now.ToDateTime());
                }
            }
        }

        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly List<IHandlerManager> handlerManagers;
        private readonly string graphitePathPrefix;
    }
}