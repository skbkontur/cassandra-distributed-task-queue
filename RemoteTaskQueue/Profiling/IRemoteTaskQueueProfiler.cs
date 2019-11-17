using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteQueue.Profiling
{
    public interface IRemoteTaskQueueProfiler
    {
        void ProcessTaskCreation([NotNull] TaskMetaInformation meta);
        void ProcessTaskExecutionFinished([NotNull] TaskMetaInformation meta, [NotNull] HandleResult handleResult, TimeSpan taskExecutionTime);
        void ProcessTaskExecutionFailed([NotNull] TaskMetaInformation meta, TimeSpan taskExecutionTime);
        void ReportLiveRecordTicksMarkerLag([NotNull] Timestamp nowTimestamp, [NotNull] LiveRecordTicksMarkerState currentLiveRecordTicksMarker);
    }
}