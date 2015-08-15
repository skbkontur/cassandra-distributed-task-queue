using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

using RemoteQueue.Handling;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class TaskQueue : ITaskQueue
    {
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

        public bool QueueTask(HandlerTask handlerTask)
        {
            lock(lockObject)
            {
                if(stopped)
                    throw new TaskQueueException("Невозможно добавить асинхронную задачу - очередь остановлена");
                if(hashtable.ContainsKey(handlerTask.TaskId))
                    return false;
                var taskWrapper = new TaskWrapper(handlerTask, this);
                var asyncTask = Task.Factory.StartNew(taskWrapper.Run);
                if(!taskWrapper.Finished)
                    hashtable.Add(handlerTask.TaskId, asyncTask);
            }
            return true;
        }

        public bool Stopped { get { return stopped; } }

        public void TaskFinished(string taskId)
        {
            lock(lockObject)
                hashtable.Remove(taskId);
            }

        private readonly Hashtable hashtable = new Hashtable();
        private readonly object lockObject = new object();
        private volatile bool stopped;
    }
}