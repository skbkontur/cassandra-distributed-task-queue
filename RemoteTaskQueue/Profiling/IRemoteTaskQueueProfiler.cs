using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling.HandlerResults;

namespace RemoteQueue.Profiling
{
    public interface IRemoteTaskQueueProfiler
    {
        void ProcessTaskCreation<T>(TaskMetaInformation meta, T taskData);
        void ProcessTaskCancel(TaskMetaInformation meta);
        void ProcessTaskCreation(TaskMetaInformation meta);
        void ProcessTaskEnqueueing(TaskMetaInformation meta);
        void ProcessTaskDequeueing(TaskMetaInformation meta);
        void ProcessTaskExecutionFinished(TaskMetaInformation meta, HandleResult handleResult, TimeSpan taskExecutionTime);
        void ProcessTaskExecutionFailed(TaskMetaInformation meta, Exception e);
    }
}