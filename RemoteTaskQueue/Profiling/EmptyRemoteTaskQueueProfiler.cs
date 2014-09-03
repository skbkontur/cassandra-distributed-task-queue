using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling.HandlerResults;

namespace RemoteQueue.Profiling
{
    public class EmptyRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public void ProcessTaskEnqueueing(TaskMetaInformation meta)
        {
        }

        public void ProcessTaskDequeueing(TaskMetaInformation meta)
        {
        }

        public void RecordTaskExecutionTime(TaskMetaInformation meta, TimeSpan taskExecutionTime)
        {
        }

        public void RecordTaskExecutionResult(TaskMetaInformation meta, HandleResult handleResult)
        {
        }
    }
}