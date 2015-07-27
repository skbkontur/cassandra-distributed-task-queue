using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

using Kontur.Tracing;
using Kontur.Tracing.EdiVersion;
using Trace = Kontur.Tracing.EdiVersion.Trace;

using RemoteQueue.Tracing;

using TraceContext = Kontur.Tracing.TraceContext;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class TaskQueue : ITaskQueue
    {
        public void Start()
        {
            lock (lockObject)
                stopped = false;

            if(!Trace.IsInitialized)
                Trace.Initialize(new TracingConfigurationProvider());
        }

        public void StopAndWait(int timeout = 10000)
        {
            if (stopped)
                return;
            Task[] tasks;
            lock (lockObject)
            {
                if (stopped)
                    return;
                stopped = true;
                tasks = hashtable.Values.Cast<Task>().ToArray();
                hashtable.Clear();
            }
            Task.WaitAll(tasks, TimeSpan.FromMilliseconds(timeout));
        }

        public long GetQueueLength()
        {
            lock (lockObject)
                return hashtable.Count;
        }

        public bool QueueTask(ITask task)
        {
            lock (lockObject)
            {
                if (stopped)
                    throw new TaskQueueException(string.Format("Невозможно добавить асинхронную задачу - очередь остановлена"));
                //traceContext = Trace.CreateChildContext("Infrastructure");
                //traceContext.RecordTimepoint(Timepoint.ServerReceive);
                if (hashtable.ContainsKey(task.Id)) return false;
                var taskWrapper = new TaskWrapper(task, this);
                var asyncTask = Task.Factory.StartNew(taskWrapper.Run);
                if (!taskWrapper.Finished)
                    hashtable.Add(task.Id, asyncTask);
            }
            return true;
        }

        public bool Stopped { get { return stopped; } }

        public void TaskFinished(ITask task)
        {
            lock (lockObject)
            {
                if (hashtable.ContainsKey(task.Id))
                    hashtable.Remove(task.Id);
                //traceContext.RecordTimepoint(Timepoint.ServerSend);
                
                //Trace.FinishCurrentContext();
            }
        }

        private readonly Hashtable hashtable = new Hashtable();
        private readonly object lockObject = new object();
        private volatile bool stopped;
        //private ITraceContext traceContext;
    }
}