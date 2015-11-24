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
        public RemoteTaskWithContinuationOptimization([NotNull] Task task, [NotNull] IHandleTaskCollection handleTaskCollection, [NotNull] ILocalTaskQueue localTaskQueue)
            : base(task, handleTaskCollection)
        {
            this.localTaskQueue = localTaskQueue;
        }

        [NotNull]
        public override sealed string Queue(TimeSpan delay)
        {
            using(new RemoteTaskInitialTraceContext(task.Meta))
            {
                var taskInfo = Publish(delay);
                bool queueIsFull, taskIsSentToThreadPool;
                localTaskQueue.QueueTask(task.Meta.Id, taskInfo, task.Meta, TaskQueueReason.TaskContinuation, out queueIsFull, out taskIsSentToThreadPool, taskIsBeingTraced : true);
                return Id;
            }
        }

        private readonly ILocalTaskQueue localTaskQueue;
    }
}