using System.Collections.Concurrent;
using System.Collections.Generic;

using log4net;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.NewEventsCounters;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class CompositeCounter : ICompositeCounter
    {
        public CompositeCounter()
        {
            newTasksCounter = new NewTasksCounter();
        }

        private NewTasksCounter newTasksCounter;

        public void ProcessMetas(TaskMetaInformation[] metas, long readTicks)
        {
            newTasksCounter.NewMetainformationAvailable(metas, readTicks);
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
            return totalCounter.GetCount();
        }

        public void Reset()
        {
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
            return new CompositeCounterSnapshot()
                {
                    Snapshots = snapshots,
                    TotalSnapshot = totalSnapshot
                };
        }

        public void LoadSnapshot(CompositeCounterSnapshot snapshot)
        {
            logger.LogInfoFormat("Loading snapshot");
            Reset();
            if(snapshot == null || snapshot.Snapshots == null || snapshot.Snapshots.Count == 0)
            {
                logger.LogWarnFormat("Snapshot is empty");
                return;
            }
            totalCounter.Reset();

            foreach(var kvp in snapshot.Snapshots)
            {
                var name = kvp.Key;
                GetCounter(name).LoadSnapshot(kvp.Value);
            }
            if(snapshot.TotalSnapshot != null)
                totalCounter.LoadSnapshot(snapshot.TotalSnapshot);

            logger.LogInfoFormat("Snapshot loaded. Counter start value = {0}", totalCounter.GetCount().Count);
        }

        private static readonly ILog logger = LogManager.GetLogger("CompositeCounter");

        private readonly ConcurrentDictionary<string, ProcessedTasksCounter> counters = new ConcurrentDictionary<string, ProcessedTasksCounter>();
        private readonly ProcessedTasksCounter totalCounter = CreateCounter();
    }
}