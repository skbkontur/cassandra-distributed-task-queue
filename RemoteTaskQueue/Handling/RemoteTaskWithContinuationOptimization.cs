using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.LocalTasks.TaskQueue;

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
            var taskInfo = Publish(delay);
            localTaskQueue.QueueTask(task.Meta.Id, taskInfo, task.Meta, TaskQueueReason.TaskContinuation);
            return Id;
        }

        private readonly ILocalTaskQueue localTaskQueue;
    }
}