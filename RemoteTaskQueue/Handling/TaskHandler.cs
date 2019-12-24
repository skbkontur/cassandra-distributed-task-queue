using System;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Tracing;

using SkbKontur.Cassandra.DistributedLock;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Handling
{
    public abstract class TaskHandler<T> : ITaskHandler where T : ITaskData
    {
        public virtual HandleResult HandleTask(IRemoteTaskQueue queue, ISerializer serializer, IRemoteLockCreator remoteLockCreator, Task task)
        {
            taskQueue = queue;
            Context = task.Meta;
            var taskData = serializer.Deserialize<T>(task.Data);
            using (new BusinessLogicTaskTraceContext())
                return HandleTask(taskData);
        }

        protected IRemoteTask CreateNextTask(ITaskData data)
        {
            return taskQueue.CreateTask(data, new CreateTaskOptions {ParentTaskId = Context.Id});
        }

        protected IRemoteTask CreateNextTask(ITaskData data, string taskGroupLock)
        {
            return taskQueue.CreateTask(data, new CreateTaskOptions {ParentTaskId = Context.Id, TaskGroupLock = taskGroupLock});
        }

        protected string ContinueWith(ITaskData data)
        {
            return CreateNextTask(data).Queue();
        }

        protected string ContinueWith(ITaskData data, TimeSpan delay)
        {
            if (delay.Ticks < 0)
                throw new InvalidProgramStateException(string.Format("Invalid delay: {0}", delay));
            return CreateNextTask(data).Queue(delay);
        }

        protected HandleResult Finish()
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Finish
                };
        }

        protected HandleResult Rerun(TimeSpan rerunDelay)
        {
            if (rerunDelay.Ticks < 0)
                throw new InvalidProgramStateException(string.Format("Invalid rerun delay: {0}", rerunDelay));
            return new HandleResult
                {
                    FinishAction = FinishAction.Rerun,
                    RerunDelay = rerunDelay
                };
        }

        protected HandleResult RerunAfterError(Exception e, TimeSpan rerunDelay)
        {
            if (rerunDelay.Ticks < 0)
                throw new InvalidProgramStateException(string.Format("Invalid rerun delay: {0}", rerunDelay));
            return new HandleResult
                {
                    FinishAction = FinishAction.RerunAfterError,
                    RerunDelay = rerunDelay,
                    Error = e
                };
        }

        protected HandleResult Fatal(Exception e)
        {
            return new HandleResult
                {
                    FinishAction = FinishAction.Fatal,
                    Error = e
                };
        }

        protected abstract HandleResult HandleTask(T taskData);
        protected TaskMetaInformation Context { get; private set; }
        private IRemoteTaskQueue taskQueue;
    }
}