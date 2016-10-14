using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace RemoteQueue.Profiling
{
    public class EmptyRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public void ProcessTaskCreation(TaskMetaInformation meta)
        {
        }

        public void ProcessTaskExecutionFinished(TaskMetaInformation meta, HandleResult handleResult, TimeSpan taskExecutionTime)
        {
        }

        public void ProcessTaskExecutionFailed(TaskMetaInformation meta, Exception e, TimeSpan taskExecutionTime)
        {
        }
    }
}