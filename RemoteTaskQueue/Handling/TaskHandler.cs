using System;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.RemoteLock;
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
            if(string.IsNullOrEmpty(taskData.QueueId))
                return HandleTask(taskData);
            using(remoteLockCreator.Lock(taskData.QueueId))
                return HandleTask(taskData);
        }

        protected string ContinueWith(ITaskData data)
        {
            return taskQueue.Queue(data, Context.Id);
        }

        protected string ContinueWith(ITaskData data, TimeSpan delay)
        {
            return taskQueue.Queue(data, delay, Context.Id);
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