using System;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Profiling
{
    public interface IRemoteTaskQueueProfiler
    {
        void ProcessTaskEnqueueing(TaskMetaInformation meta);
        void ProcessTaskDequeueing(TaskMetaInformation meta);
        void RecordTaskExecutionTime(TaskMetaInformation meta, TimeSpan taskExecutionTime);
    }
}