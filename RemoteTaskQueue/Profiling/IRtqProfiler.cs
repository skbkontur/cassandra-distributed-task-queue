using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Profiling
{
    public interface IRtqProfiler
    {
        void ProcessTaskCreation([NotNull] TaskMetaInformation meta);
        void ProcessTaskExecutionFinished([NotNull] TaskMetaInformation meta, [NotNull] HandleResult handleResult, TimeSpan taskExecutionTime);
        void ProcessTaskExecutionFailed([NotNull] TaskMetaInformation meta, TimeSpan taskExecutionTime);
        void ReportLiveRecordTicksMarkerLag([NotNull] Timestamp nowTimestamp, [NotNull] LiveRecordTicksMarkerState currentLiveRecordTicksMarker);
    }
}