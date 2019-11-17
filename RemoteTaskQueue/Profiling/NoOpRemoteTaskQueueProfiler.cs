using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteQueue.Profiling
{
    public class NoOpRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public void ProcessTaskCreation([NotNull] TaskMetaInformation meta)
        {
        }

        public void ProcessTaskExecutionFinished([NotNull] TaskMetaInformation meta, [NotNull] HandleResult handleResult, TimeSpan taskExecutionTime)
        {
        }

        public void ProcessTaskExecutionFailed([NotNull] TaskMetaInformation meta, TimeSpan taskExecutionTime)
        {
        }

        public void ReportLiveRecordTicksMarkerLag([NotNull] Timestamp nowTimestamp, [NotNull] LiveRecordTicksMarkerState currentLiveRecordTicksMarker)
        {
        }
    }
}