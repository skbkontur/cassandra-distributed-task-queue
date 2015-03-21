using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class ProcessedTasksCounter 
    {
        public ProcessedTasksCounter()
        {
            Reset();
        }

        public void NewMetainformationAvailable(TaskMetaInformation[] metas, long nowTime)
        {
            lock(lockObject)
            {
                if(metas.Length <= 0)
                    NoMeta(nowTime);
                else
                {
                    foreach(var meta in metas)
                        Process(meta);
                }
            }
        }

        public void NoMeta(long nowTicks)
        {
            Interlocked.Exchange(ref lastCalculationTime, nowTicks);
        }

        public void Process(TaskMetaInformation meta)
        {
            ProcessTask(meta.Id, meta.State);
            var newTime = Math.Max(Interlocked.Read(ref lastCalculationTime), meta.LastModificationTicks.Value);
            Interlocked.Exchange(ref lastCalculationTime, newTime);
        }

        public TaskCount GetCount()
        {
            return new TaskCount {Count = Interlocked.CompareExchange(ref count, 0, 0), UpdateTicks = Interlocked.Read(ref lastCalculationTime)};
        }

        public void Reset()
        {
            lock(lockObject)
            {
                notFinishedTasks.Clear();
                Interlocked.Exchange(ref lastCalculationTime, 0);
                Interlocked.Exchange(ref count, 0);
            }
        }

        public CounterSnapshot GetSnapshotOrNull(int maxLength)
        {
            lock(lockObject)
            {
                if(notFinishedTasks.Count > maxLength)
                    return null;
                return new CounterSnapshot(notFinishedTasks, Interlocked.Read(ref lastCalculationTime), Interlocked.CompareExchange(ref count, 0, 0));
            }
        }

        public void LoadSnapshot(CounterSnapshot snapshot)
        {
            lock(lockObject)
            {
                notFinishedTasks = new HashSet<string>(snapshot.NotFinishedTasks ?? new string[0]);
                Interlocked.Exchange(ref lastCalculationTime, snapshot.CountCalculatedTime);
                Interlocked.Exchange(ref count, snapshot.Count);
            }
        }

        public int GetNotFinishedTasksCount()
        {
            lock(lockObject)
                return notFinishedTasks.Count;
        }

        private void Increment()
        {
            Interlocked.Increment(ref count);
        }

        private void Decrement()
        {
            Interlocked.Decrement(ref count);
        }

        private void ProcessTask(string taskId, TaskState newState)
        {
            var isRunning = notFinishedTasks.Contains(taskId);
            var newIsTerm = IsTerminalState(newState);
            if(isRunning)
            {
                if(newIsTerm)
                    Decrement();
            }
            else
            {
                if(!newIsTerm)
                    Increment();
            }
            if(newIsTerm)
                notFinishedTasks.Remove(taskId);
            else
                notFinishedTasks.Add(taskId);
        }

        private static bool IsTerminalState(TaskState s)
        {
            return s == TaskState.Canceled || s == TaskState.Fatal || s == TaskState.Finished;
        }

        private readonly object lockObject = new object();

        #region state

        //note решили не ограничивать количество задач, если станет много можно сбросить счетчик
        private volatile HashSet<string> notFinishedTasks = new HashSet<string>();
        private long lastCalculationTime = 0;
        private int count = 0;

        #endregion

        public class CounterSnapshot
        {
            public CounterSnapshot(HashSet<string> notFinishedTasks, long countCalculatedTime, int count)
            {
                NotFinishedTasks = notFinishedTasks == null ? null : notFinishedTasks.ToArray();
                CountCalculatedTime = countCalculatedTime;
                Count = count;
            }

            public string[] NotFinishedTasks { get; private set; }
            public long CountCalculatedTime { get; private set; }
            public int Count { get; private set; }
        }
    }
}