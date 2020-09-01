using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue;
using SkbKontur.Cassandra.DistributedTaskQueue.Tracing;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
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