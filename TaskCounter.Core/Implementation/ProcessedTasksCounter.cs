using System;
using System.Collections.Generic;
using System.Threading;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class ProcessedTasksCounter
    {
        public ProcessedTasksCounter()
        {
            SetEmptyCounts();
            stateMap = new Dictionary<string, TaskState>();
            Reset();
        }

        private void SetEmptyCounts()
        {
            counts = new int[TaskStateHelpers.statesCount];
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
            var taskCount = new TaskCount
                {
                    Count = Interlocked.CompareExchange(ref count, 0, 0),
                    UpdateTicks = Interlocked.Read(ref lastCalculationTime),
                    Counts = SafeClone(counts)
                };
            return taskCount;
        }

        public void Reset()
        {
            lock(lockObject)
            {
                stateMap.Clear();
                Interlocked.Exchange(ref lastCalculationTime, 0);
                Interlocked.Exchange(ref count, 0);
                SetEmptyCounts();
            }
        }

        public CounterSnapshot GetSnapshotOrNull(int maxLength)
        {
            lock(lockObject)
            {
                if(stateMap.Count > maxLength)
                    return null;
                return new CounterSnapshot(stateMap, Interlocked.Read(ref lastCalculationTime), Interlocked.CompareExchange(ref count, 0, 0), counts);
            }
        }

        public void LoadSnapshot(CounterSnapshot snapshot)
        {
            lock(lockObject)
            {
                stateMap = snapshot.BuildMap();
                Interlocked.Exchange(ref lastCalculationTime, snapshot.CountCalculatedTime);
                Interlocked.Exchange(ref count, snapshot.Count);
                counts = SafeClone(snapshot.Counts);
                if(counts == null)
                    SetEmptyCounts();
                if(counts.Length != TaskStateHelpers.statesCount)
                    throw new InvalidOperationException("Snaphot corrupted");
            }
        }

        public int GetNotFinishedTasksCount()
        {
            lock(lockObject)
                return stateMap.Count;
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
            TaskState oldState;
            var isRunning = stateMap.TryGetValue(taskId, out oldState);
            var newIsTerm = IsTerminalState(newState);
            if(isRunning)
            {
                if(newIsTerm)
                {
                    Decrement();
                    AddCounter(oldState, -1);
                }
                else
                {
                    AddCounter(oldState, -1);
                    AddCounter(newState, 1);
                }
            }
            else
            {
                if(!newIsTerm)
                {
                    Increment();
                    AddCounter(newState, 1);
                }
            }
            if(newIsTerm)
                stateMap.Remove(taskId);
            else
                stateMap[taskId] = newState;
        }

        private void AddCounter(TaskState state, int value)
        {
            Interlocked.Add(ref counts[(int)state], value);
        }

        private static bool IsTerminalState(TaskState s)
        {
            return s == TaskState.Canceled || s == TaskState.Fatal || s == TaskState.Finished;
        }

        private static int[] SafeClone(int[] src)
        {
            if(src == null) return null;
            var length = src.Length;
            var res = new int[length];
            for(var i = 0; i < length; i++)
                res[i] = Interlocked.CompareExchange(ref src[i], 0, 0);
            return res;
        }

        private readonly object lockObject = new object();

        #region state

        //note решили не ограничивать количество задач, если станет много можно сбросить счетчик
        private volatile int[] counts;
        private volatile Dictionary<string, TaskState> stateMap;
        private long lastCalculationTime = 0;
        private int count = 0;

        #endregion

        public class CounterSnapshot
        {
            public CounterSnapshot(Dictionary<string, TaskState> map, long countCalculatedTime, int count, int[] counts)
            {
                if(map != null)
                {
                    var i = 0;
                    TaskIds = new string[map.Count];
                    TaskStates = new TaskState[map.Count];
                    foreach(var kvp in map)
                    {
                        TaskIds[i] = kvp.Key;
                        TaskStates[i] = kvp.Value;
                        ++i;
                    }
                }
                CountCalculatedTime = countCalculatedTime;
                Count = count;
                Counts = SafeClone(counts);
            }

            public Dictionary<string, TaskState> BuildMap()
            {
                var res = new Dictionary<string, TaskState>();
                if(TaskIds != null && TaskStates != null)
                {
                    if(TaskIds.Length != TaskStates.Length)
                        throw new InvalidOperationException("Snaphot currupted. Lengths are not same");
                    for(var i = 0; i < TaskIds.Length; i++)
                        res.Add(TaskIds[i], TaskStates[i]);
                }
                return res;
            }

            public string[] TaskIds { get; private set; }
            public TaskState[] TaskStates { get; private set; }

            public long CountCalculatedTime { get; private set; }
            public int Count { get; private set; }
            public int[] Counts { get; set; }
        }
    }
}