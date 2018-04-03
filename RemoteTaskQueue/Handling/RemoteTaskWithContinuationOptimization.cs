using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Tracing;

namespace RemoteQueue.Handling
{
    internal class RemoteTaskWithContinuationOptimization : RemoteTask
    {
        public RemoteTaskWithContinuationOptimization([NotNull] Task task, TimeSpan taskTtl, [NotNull] IHandleTaskCollection handleTaskCollection, [NotNull] ILocalTaskQueue localTaskQueue)
            : base(task, taskTtl, handleTaskCollection)
        {
            this.localTaskQueue = localTaskQueue;
        }

        [NotNull]
        public override sealed string Queue(TimeSpan delay)
        {
            using (new RemoteTaskInitialTraceContext(task.Meta))
            {
                var taskIndexRecord = Publish(delay);
                if (delay == TimeSpan.Zero)
                    localTaskQueue.TryQueueTask(taskIndexRecord, task.Meta, TaskQueueReason.TaskContinuation, taskIsBeingTraced : true);
                return Id;
            }
        }

        private readonly ILocalTaskQueue localTaskQueue;
    }
}