using System;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling.HandlerResults;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;

using Kontur.Tracing.Core;


namespace RemoteQueue.Handling
{
    public abstract class TaskHandler<T> : ITaskHandler where T : ITaskData
    {
        public virtual HandleResult HandleTask(IRemoteTaskQueue queue, ISerializer serializer, IRemoteLockCreator remoteLockCreator, Task task)
        {
            taskQueue = queue;
            Context = task.Meta;
            var taskData = serializer.Deserialize<T>(task.Data);
            HandleResult handleResult;
            using(var traceContext = Trace.CreateChildContext("Handling"))
            {
                traceContext.RecordTimepoint(Timepoint.ServerReceive);
                handleResult = HandleTask(taskData);
                traceContext.RecordTimepoint(Timepoint.ServerSend);
            }
            return handleResult;
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