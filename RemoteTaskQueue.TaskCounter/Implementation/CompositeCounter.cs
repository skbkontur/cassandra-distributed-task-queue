using System.Collections.Concurrent;
using System.Collections.Generic;

using log4net;

using RemoteQueue.Cassandra.Entities;

using RemoteTaskQueue.TaskCounter.Implementation.OldWaitingTasksCounters;
using RemoteTaskQueue.TaskCounter.Implementation.Utils;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class CompositeCounter : ICompositeCounter
    {
        public CompositeCounter(OldWaitingTasksCounter oldWaitingTasksCounter)
        {
            this.oldWaitingTasksCounter = oldWaitingTasksCounter;
        }

        public void ProcessMetas(TaskMetaInformation[] metas, long readTicks)
        {
            oldWaitingTasksCounter.NewMetainformationAvailable(metas, readTicks);
            var processedNames = new HashSet<string>();
            foreach(var taskMetaInformation in metas)
            {
                if(!string.IsNullOrEmpty(taskMetaInformation.Name))
                {
                    totalCounter.Process(taskMetaInformation);
                    processedNames.Add(taskMetaInformation.Name);
                    GetCounter(taskMetaInformation.Name).Process(taskMetaInformation);
                }
            }
            foreach(var kvp in counters)
            {
                if(!processedNames.Contains(kvp.Key))
                    kvp.Value.NoMeta(readTicks);
            }
            if(metas.Length <= 0)
                totalCounter.NoMeta(readTicks);
        }

        private ProcessedTasksCounter GetCounter(string name)
        {
            var value = counters.GetOrAdd(name, s => new ProcessedTasksCounter());
            return value;
        }

        private static ProcessedTasksCounter CreateCounter()
        {
            return new ProcessedTasksCounter();
        }

        public Dictionary<string, TaskCount> GetAllCounts()
        {
            var result = new Dictionary<string, TaskCount>();
            foreach(var kvp in counters)
            {
                var taskCount = kvp.Value.GetCount();
                result.Add(kvp.Key, taskCount);
            }
            return result;
        }

        public TaskCount GetTotalCount()
        {
            var totalCount = totalCounter.GetCount();
            totalCount.OldWaitingTaskCount = oldWaitingTasksCounter.GetValue();
            return totalCount;
        }

        public void Reset()
        {
            oldWaitingTasksCounter.Reset();
            totalCounter.Reset();
            counters.Clear();
        }

        public CompositeCounterSnapshot GetSnapshotOrNull(int maxLength)
        {
            var snapshots = new Dictionary<string, ProcessedTasksCounter.CounterSnapshot>();
            foreach(var kvp in counters)
            {
                var counter = kvp.Value;
                var snapshot = counter.GetSnapshotOrNull(maxLength);
                if(snapshot == null)
                {
                    logger.LogWarnFormat("Snapshot for TaskName={0} is big", kvp.Key);
                    return null;
                }
                snapshots.Add(kvp.Key, snapshot);
            }
            var totalSnapshot = totalCounter.GetSnapshotOrNull(maxLength);
            if(totalSnapshot == null)
            {
                logger.LogWarnFormat("Total snapshot is big");
                return null;
            }
            var oldWaitingCounterSnapshot = oldWaitingTasksCounter.GetSnapshot(maxLength);
            return new CompositeCounterSnapshot()
                {
                    Snapshots = snapshots,
                    TotalSnapshot = totalSnapshot,
                    OldWaitingCounterSnapshot = oldWaitingCounterSnapshot
                };
        }

        public void LoadSnapshot(CompositeCounterSnapshot snapshot)
        {
            logger.LogInfoFormat("Loading snapshot");
            Reset();
            oldWaitingTasksCounter.Reset();
            totalCounter.Reset();
            if(snapshot == null)
            {
                logger.LogWarnFormat("Snapshot is empty");
                return;
            }

            if(snapshot.Snapshots != null && snapshot.Snapshots.Count != 0)
            {
                foreach(var kvp in snapshot.Snapshots)
                {
                    var name = kvp.Key;
                    GetCounter(name).LoadSnapshot(kvp.Value);
                }
            }
            else
                logger.LogWarnFormat("Per-counter snapshots are empty");
            if(snapshot.TotalSnapshot != null)
                totalCounter.LoadSnapshot(snapshot.TotalSnapshot);
            else
                logger.LogWarnFormat("totalCounter snapshot is empty");

            if(snapshot.OldWaitingCounterSnapshot != null)
                oldWaitingTasksCounter.LoadSnapshot(snapshot.OldWaitingCounterSnapshot);
            else
                logger.LogWarnFormat("oldWaitingTasksCounter snapshot is empty");

            logger.LogInfoFormat("Snapshot loaded. Counter start value = {0}", totalCounter.GetCount().Count);
        }

        private readonly OldWaitingTasksCounter oldWaitingTasksCounter;

        private static readonly ILog logger = LogManager.GetLogger("CompositeCounter");

        private readonly ConcurrentDictionary<string, ProcessedTasksCounter> counters = new ConcurrentDictionary<string, ProcessedTasksCounter>();
        private readonly ProcessedTasksCounter totalCounter = CreateCounter();
    }
}