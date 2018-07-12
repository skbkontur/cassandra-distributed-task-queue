using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace RemoteQueue.Profiling
{
    public class EmptyRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
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
    }
}