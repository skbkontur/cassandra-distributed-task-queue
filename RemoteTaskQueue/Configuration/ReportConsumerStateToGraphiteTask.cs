using System;
using System.Collections.Generic;

using RemoteQueue.Handling;
using RemoteQueue.Profiling;

using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteQueue.Configuration
{
    public class ReportConsumerStateToGraphiteTask : PeriodicTaskBase
    {
        public ReportConsumerStateToGraphiteTask(IRemoteTaskQueueProfiler remoteTaskQueueProfiler, List<IHandlerManager> handlerManagers)
        {
            this.remoteTaskQueueProfiler = remoteTaskQueueProfiler;
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
                    remoteTaskQueueProfiler.ReportLiveRecordTicksMarkerLag(nowTimestamp, currentLiveRecordTicksMarker);
            }
        }

        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
        private readonly List<IHandlerManager> handlerManagers;
        private readonly Timestamp startupTimestamp;
    }
}