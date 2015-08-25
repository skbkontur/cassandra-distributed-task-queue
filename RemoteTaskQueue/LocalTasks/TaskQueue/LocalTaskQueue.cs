using System;
using System.Collections;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Handling;

using Task = System.Threading.Tasks.Task;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class LocalTaskQueue : ILocalTaskQueue
    {
        public LocalTaskQueue(ITaskCounter taskCounter, ITaskHandlerCollection taskHandlerCollection, IRemoteTaskQueueInternals remoteTaskQueueInternals)
        {
            this.taskCounter = taskCounter;
            this.taskHandlerCollection = taskHandlerCollection;
            this.remoteTaskQueueInternals = remoteTaskQueueInternals;
            Instance = this;
        }

        // NB! адская статика, чтобы внутри процессов consumer'ов continuation-оптимизация работала неявно
        [CanBeNull]
        public static ILocalTaskQueue Instance { get; private set; }

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

        public void QueueTask([NotNull] string taskId, [NotNull] ColumnInfo taskInfo, [CanBeNull] TaskMetaInformation taskMeta, TaskQueueReason taskQueueReason, out bool queueIsFull)
        {
            queueIsFull = false;
            if(taskMeta != null && !taskHandlerCollection.ContainsHandlerFor(taskMeta.Name))
                return;
            if(!taskCounter.TryIncrement(taskQueueReason))
            {
                queueIsFull = true;
                return;
            }
            var taskIsSentToThreadPool = false;
            try
            {
                var handlerTask = new HandlerTask(taskId, taskQueueReason, taskInfo, taskMeta, taskHandlerCollection, remoteTaskQueueInternals);
                lock(lockObject)
                {
                    if(stopped)
                        throw new TaskQueueException("Невозможно добавить асинхронную задачу - очередь остановлена");
                    if(hashtable.ContainsKey(taskId))
                        return;
                    var taskWrapper = new TaskWrapper(taskId, taskQueueReason, handlerTask, this);
                    var asyncTask = Task.Factory.StartNew(taskWrapper.Run);
                    taskIsSentToThreadPool = true;
                    if(!taskWrapper.Finished)
                        hashtable.Add(taskId, asyncTask);
                }
            }
            finally
            {
                if(!taskIsSentToThreadPool)
                    taskCounter.Decrement(taskQueueReason);
            }
        }

        public void TaskFinished([NotNull] string taskId, TaskQueueReason taskQueueReason)
        {
            lock(lockObject)
                hashtable.Remove(taskId);
            taskCounter.Decrement(taskQueueReason);
        }

        private readonly ITaskCounter taskCounter;
        private readonly ITaskHandlerCollection taskHandlerCollection;
        private readonly IRemoteTaskQueueInternals remoteTaskQueueInternals;
        private readonly Hashtable hashtable = new Hashtable();
        private readonly object lockObject = new object();
        private volatile bool stopped;
    }
}