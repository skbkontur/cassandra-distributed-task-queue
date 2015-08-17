using System;
using System.Collections;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Handling;
using RemoteQueue.Tracing;

using Task = System.Threading.Tasks.Task;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class LocalTaskQueue : ILocalTaskQueue
    {
        public LocalTaskQueue(Func<string, TaskQueueReason, ColumnInfo, TaskMetaInformation, HandlerTask> createHandlerTask)
        {
            this.createHandlerTask = createHandlerTask;
        }

        public void Start()
        {
            lock(lockObject)
                stopped = false;
        }

        public void StopAndWait(int timeout = 10000)
        {
            if(stopped)
                return;
            Task[] tasks;
            lock(lockObject)
            {
                if(stopped)
                    return;
                stopped = true;
                tasks = hashtable.Values.Cast<Task>().ToArray();
                hashtable.Clear();
            }
            Task.WaitAll(tasks, TimeSpan.FromMilliseconds(timeout));
        }

        public long GetQueueLength()
        {
            lock(lockObject)
                return hashtable.Count;
        }

        public void QueueTask(ColumnInfo taskInfo, TaskMetaInformation taskMeta, TaskQueueReason taskQueueReason)
        {
            using(var infrastructureTraceContext = new InfrastructureTaskTraceContext())
            {
                var alreadyInQueue = false;
                var taskId = taskMeta.Id;
                var handlerTask = createHandlerTask(taskId, taskQueueReason, taskInfo, taskMeta);
                lock(lockObject)
                {
                    if(stopped)
                        throw new TaskQueueException("Невозможно добавить асинхронную задачу - очередь остановлена");
                    if(hashtable.ContainsKey(taskId))
                        alreadyInQueue = true;
                    else
                    {
                        var taskWrapper = new TaskWrapper(taskId, handlerTask, this);
                        var asyncTask = Task.Factory.StartNew(taskWrapper.Run);
                        if(!taskWrapper.Finished)
                            hashtable.Add(taskId, asyncTask);
                    }
                }
                if(alreadyInQueue)
                    infrastructureTraceContext.RecordFinish();
            }
        }

        public void TaskFinished(string taskId, LocalTaskProcessingResult result)
        {
            lock(lockObject)
                hashtable.Remove(taskId);
            InfrastructureTaskTraceContext.Finish();
            RemoteTaskHandlingTraceContext.Finish(result);
        }

        private readonly Func<string, TaskQueueReason, ColumnInfo, TaskMetaInformation, HandlerTask> createHandlerTask;
        private readonly Hashtable hashtable = new Hashtable();
        private readonly object lockObject = new object();
        private volatile bool stopped;
    }
}