using System;
using System.Collections.Generic;

using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    internal class ReportConsumerStateToGraphiteTask : PeriodicTaskBase
    {
        public ReportConsumerStateToGraphiteTask(IRtqProfiler rtqProfiler, List<IHandlerManager> handlerManagers)
        {
            this.rtqProfiler = rtqProfiler;
            this.handlerManagers = handlerManagers;
            startupTimestamp = Timestamp.Now;
        }

        public override sealed void Run()
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