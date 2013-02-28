using System;

using GroBuf;

using RemoteLock;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling.HandlerResults;

namespace RemoteQueue.Handling
{
    public abstract class TaskHandler<T> : ITaskHandler where T : ITaskData
    {
        public HandleResult HandleTask(IRemoteTaskQueue queue, ISerializer serializer, IRemoteLockCreator remoteLockCreator, Task task)
        {
            taskQueue = queue;
            Context = task.Meta;
            var taskData = serializer.Deserialize<T>(task.Data);
            return HandleTask(taskData);
        }

        protected string ContinueWith(ITaskData data)
        {
            return taskQueue.CreateTask(data, new CreateTaskOptions{ParentTaskId = Context.Id}).Queue();
        }

        protected string ContinueWith(ITaskData data, TimeSpan delay)
        {
            return taskQueue.CreateTask(data, new CreateTaskOptions { ParentTaskId = Context.Id }).Queue(delay);
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
            return new HandleResult
                {
                    FinishAction = FinishAction.Rerun,
                    RerunDelay = rerunDelay
                };
        }

        protected HandleResult RerunAfterError(Exception e, TimeSpan rerunDelay)
        {
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