using System;

using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Tracing;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public abstract class RtqTaskHandler<T> : IRtqTaskHandler
        where T : IRtqTaskData
    {
        [NotNull]
        public virtual HandleResult HandleTask([NotNull] IRtqTaskProducer taskProducer, [NotNull] ISerializer serializer, [NotNull] Task task)
        {
            theTaskProducer = taskProducer;
            Context = task.Meta;
            var taskData = serializer.Deserialize<T>(task.Data);
            using (new BusinessLogicTaskTraceContext())
                return HandleTask(taskData);
        }

        [NotNull]
        protected IRemoteTask CreateNextTask([NotNull] IRtqTaskData taskData)
        {
            return theTaskProducer.CreateTask(taskData, new CreateTaskOptions {ParentTaskId = Context.Id});
        }

        [NotNull]
        protected IRemoteTask CreateNextTask([NotNull] IRtqTaskData taskData, [NotNull] string taskGroupLock)
        {
            return theTaskProducer.CreateTask(taskData, new CreateTaskOptions {ParentTaskId = Context.Id, TaskGroupLock = taskGroupLock});
        }

        [NotNull]
        protected string ContinueWith([NotNull] IRtqTaskData taskData)
        {
            return CreateNextTask(taskData).Queue();
        }

        [NotNull]
        protected string ContinueWith([NotNull] IRtqTaskData taskData, TimeSpan delay)
        {
            if (delay.Ticks < 0)
                throw new InvalidOperationException($"Invalid delay: {delay}");
            return CreateNextTask(taskData).Queue(delay);
        }

        [NotNull]
        protected HandleResult Finish()
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Finish
                };
        }

        [NotNull]
        protected HandleResult Rerun(TimeSpan rerunDelay)
        {
            if (rerunDelay.Ticks < 0)
                throw new InvalidOperationException($"Invalid rerun delay: {rerunDelay}");
            return new HandleResult
                {
                    FinishAction = FinishAction.Rerun,
                    RerunDelay = rerunDelay
                };
        }

        [NotNull]
        protected HandleResult RerunAfterError([NotNull] Exception e, TimeSpan rerunDelay)
        {
            if (rerunDelay.Ticks < 0)
                throw new InvalidOperationException($"Invalid rerun delay: {rerunDelay}");
            return new HandleResult
                {
                    FinishAction = FinishAction.RerunAfterError,
                    RerunDelay = rerunDelay,
                    Error = e ?? throw new InvalidOperationException("Exception is required")
                };
        }

        [NotNull]
        protected HandleResult Fatal([NotNull] Exception e)
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Fatal,
                    Error = e ?? throw new InvalidOperationException("Exception is required")
                };
        }

        [NotNull]
        protected abstract HandleResult HandleTask([NotNull] T taskData);

        [NotNull]
        protected TaskMetaInformation Context { get; private set; }

        private IRtqTaskProducer theTaskProducer;
    }
}