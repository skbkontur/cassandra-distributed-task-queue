using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling.HandlerResults;

namespace RemoteQueue.Profiling
{
    public interface IRemoteTaskQueueProfiler
    {
        void ProcessTaskEnqueueing(TaskMetaInformation meta);
        void ProcessTaskDequeueing(TaskMetaInformation meta);
        void RecordTaskExecutionTime(TaskMetaInformation meta, TimeSpan taskExecutionTime);
        void RecordTaskExecutionResult(TaskMetaInformation meta, HandleResult handleResult);
    }
}