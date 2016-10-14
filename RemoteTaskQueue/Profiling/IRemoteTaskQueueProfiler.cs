using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace RemoteQueue.Profiling
{
    public interface IRemoteTaskQueueProfiler
    {
        void ProcessTaskCreation(TaskMetaInformation meta);
        void ProcessTaskExecutionFinished(TaskMetaInformation meta, HandleResult handleResult, TimeSpan taskExecutionTime);
        void ProcessTaskExecutionFailed(TaskMetaInformation meta, Exception e, TimeSpan taskExecutionTime);
    }
}