using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    internal class ReportConsumerStateToGraphitePeriodicJob
    {
        public ReportConsumerStateToGraphitePeriodicJob([NotNull] string queueKeyspace, IRtqProfiler rtqProfiler, List<IHandlerManager> handlerManagers)
        {
            JobId = $"{GetType().Name}_{queueKeyspace}";
            this.rtqProfiler = rtqProfiler;
            this.handlerManagers = handlerManagers;
            startupTimestamp = Timestamp.Now;
        }

        [NotNull]
        public string JobId { get; }

        public void RunJobIteration()
        {
            var nowTimestamp = Timestamp.Now;
            if (nowTimestamp - startupTimestamp < TimeSpan.FromMinutes(3))
                return;

            foreach (var handlerManager in handlerManagers)
            {
                foreach (var currentLiveRecordTicksMarker in handlerManager.GetCurrentLiveRecordTicksMarkers())
                    rtqProfiler.ReportLiveRecordTicksMarkerLag(nowTimestamp, currentLiveRecordTicksMarker);
            }
        }

        private readonly IRtqProfiler rtqProfiler;
        private readonly List<IHandlerManager> handlerManagers;
        private readonly Timestamp startupTimestamp;
    }
}