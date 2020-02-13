using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Tracing;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Objects;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    internal class RemoteTask : IRemoteTask
    {
        public RemoteTask([NotNull] Task task, TimeSpan taskTtl, IHandleTaskCollection handleTaskCollection)
        {
            this.task = task;
            this.taskTtl = taskTtl;
            this.handleTaskCollection = handleTaskCollection;
        }

        [NotNull]
        public string Id => task.Meta.Id;

        [CanBeNull]
        public string ParentTaskId => task.Meta.ParentTaskId;

        [NotNull]
        public string Queue()
        {
            return Queue(TimeSpan.FromTicks(0));
        }

        [NotNull]
        public virtual string Queue(TimeSpan delay)
        {
            using (new RemoteTaskInitialTraceContext(task.Meta))
            {
                Publish(delay);
                return Id;
            }
        }

        [NotNull]
        protected TaskIndexRecord Publish(TimeSpan delay)
        {
            if (delay.Ticks < 0)
                throw new InvalidProgramStateException(string.Format("Invalid delay: {0}", delay));
            using (new PublishTaskTraceContext())
            {
                task.Meta.MinimalStartTicks = (Timestamp.Now + delay).Ticks;
                task.Meta.SetOrUpdateTtl(taskTtl);
                return handleTaskCollection.AddTask(task);
            }
        }

        protected readonly Task task;
        private readonly TimeSpan taskTtl;
        private readonly IHandleTaskCollection handleTaskCollection;
    }
}