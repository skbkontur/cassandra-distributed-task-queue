using System;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Scheduling;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;
using SkbKontur.Graphite.Client;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.EventFeeds
{
    [UsedImplicitly]
    public class EventFeedsGraphiteLagReporter
    {
        public EventFeedsGraphiteLagReporter(IGraphiteClient graphiteClient, IPeriodicTaskRunner periodicTaskRunner)
        {
            this.graphiteClient = graphiteClient;
            this.periodicTaskRunner = periodicTaskRunner;
            graphitePathPrefix = $"SubSystem.EventFeeds.ActualizationLag.{Environment.MachineName}";
        }

        public void Start([NotNull] IRunningEventFeed runningEventFeed)
        {
            periodicTaskRunner.Register(new ActionPeriodicTask(FormatLagReportingJobName(runningEventFeed.FeedKey),
                                                               () => ReportActualizationLagToGraphite(runningEventFeed)),
                                        TimeSpan.FromMinutes(1));
        }

        public void Stop([NotNull] IRunningEventFeed runningEventFeed)
        {
            periodicTaskRunner.Unregister(FormatLagReportingJobName(runningEventFeed.FeedKey), timeout : 15000);
        }

        [NotNull]
        private static string FormatLagReportingJobName([NotNull] string feedKey)
        {
            return $"{feedKey}-ReportActualizationLagJob";
        }

        private void ReportActualizationLagToGraphite([NotNull] IRunningEventFeed runningEventFeed)
        {
            var offsetsToReport = runningEventFeed.GetCurrentGlobalOffsetTimestamps()
                                                  .Where(t => t.CurrentGlobalOffsetTimestamp != null)
                                                  .ToArray();
            var now = Timestamp.Now;
            foreach (var t in offsetsToReport)
            {
                var graphitePath = $"{graphitePathPrefix}.{t.BladeId.BladeKey}";
                graphiteClient.Send(graphitePath, (long)(now - t.CurrentGlobalOffsetTimestamp).TotalMilliseconds, now.ToDateTime());
            }
        }

        private readonly IGraphiteClient graphiteClient;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly string graphitePathPrefix;
    }
}