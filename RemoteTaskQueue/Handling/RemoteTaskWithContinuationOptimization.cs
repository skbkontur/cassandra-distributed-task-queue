using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.LocalTasks.TaskQueue;

namespace RemoteQueue.Handling
{
    internal class RemoteTaskWithContinuationOptimization : RemoteTask
    {
        public RemoteTaskWithContinuationOptimization(Task task, IHandleTaskCollection handleTaskCollection, ILocalTaskQueue localTaskQueue)
            : base(task, handleTaskCollection)
        {
            this.localTaskQueue = localTaskQueue;
        }

        public override sealed string Queue(TimeSpan delay)
        {
            var taskInfo = WriteTaskMeta(delay);
            localTaskQueue.QueueTask(taskInfo, task.Meta, TaskQueueReason.TaskContinuation);
            return Id;
        }

        private readonly ILocalTaskQueue localTaskQueue;
    }
}