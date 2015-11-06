using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling.HandlerResults;

namespace RemoteQueue.Profiling
{
    public class EmptyRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public void ProcessTaskCreation<T>(TaskMetaInformation meta, T taskData)
        {
        }

        public void ProcessTaskCancel(TaskMetaInformation meta)
        {
        }

        public void ProcessTaskCreation(TaskMetaInformation meta)
        {
        }

        public void ProcessTaskEnqueueing(TaskMetaInformation meta)
        {
        }

        public void ProcessTaskDequeueing(TaskMetaInformation meta)
        {
        }

        public void ProcessTaskExecutionFinished(TaskMetaInformation meta, HandleResult handleResult, TimeSpan taskExecutionTime)
        {
        }

        public void ProcessTaskExecutionFailed(TaskMetaInformation meta, Exception e)
        {
        }
    }
}