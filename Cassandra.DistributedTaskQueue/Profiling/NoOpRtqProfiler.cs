using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Profiling
{
    public class NoOpRtqProfiler : IRtqProfiler
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