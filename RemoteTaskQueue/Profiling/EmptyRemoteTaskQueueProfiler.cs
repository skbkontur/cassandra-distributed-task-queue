﻿using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

namespace RemoteQueue.Profiling
{
    public class EmptyRemoteTaskQueueProfiler : IRemoteTaskQueueProfiler
    {
        public void ProcessTaskCreation(TaskMetaInformation meta)
        {
        }

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