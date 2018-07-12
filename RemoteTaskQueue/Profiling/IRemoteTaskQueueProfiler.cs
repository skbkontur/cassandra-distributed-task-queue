using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace RemoteQueue.Profiling
{
    public interface IRemoteTaskQueueProfiler
    {
        void ProcessTaskCreation([NotNull] TaskMetaInformation meta);
        void ProcessTaskExecutionFinished([NotNull] TaskMetaInformation meta, [NotNull] HandleResult handleResult, TimeSpan taskExecutionTime);
        void ProcessTaskExecutionFailed([NotNull] TaskMetaInformation meta, TimeSpan taskExecutionTime);
    }
}