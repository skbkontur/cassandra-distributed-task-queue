using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GroboContainer.Infection;

using log4net;

using RemoteQueue.Cassandra.Entities;

using RemoteTaskQueue.TaskCounter.Implementation.Utils;

using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.TaskCounter.Implementation.OldWaitingTasksCounters
{
    public class OldWaitingTasksCounter
    {
        public OldWaitingTasksCounter(long watchInterval)
        {
            notCountedNewTasks = new Dictionary<string, long>();
            this.watchInterval = watchInterval;
            value = 0;
        }
        
        [ContainerConstructor]
        public OldWaitingTasksCounter()
            : this(CounterSettings.NewEventsWatchInterval.Ticks)
        {
        }

        private static bool IsWaitingState(TaskMetaInformation meta)
        {
            return meta.State == TaskState.New || meta.State == TaskState.Unknown || meta.State == TaskState.WaitingForRerun || meta.State == TaskState.WaitingForRerunAfterError;
        }

        public void Reset()
        {
            lock(lockObject)
            {
                logger.LogInfoFormat("Reset");
                tasks.Clear();
                notCountedNewTasks = new Dictionary<string, long>();
                SetValue(0);
            }
        }

        public void LoadSnapshot(OldWaitingCounterSnapshot snapshot)
        {
            if(snapshot == null || snapshot.Tasks == null)
            {
                logger.LogInfoFormat("Snapshot is empty");
                return;
            }
            lock(lockObject)
            {
                Reset();
                foreach(var task in snapshot.Tasks)
                    tasks.Add(task);
                if(snapshot.NotCountedNewTasks != null)
                    notCountedNewTasks = new Dictionary<string, long>(snapshot.NotCountedNewTasks);
                SetValue(tasks.Count);
            }
        }

        public OldWaitingCounterSnapshot GetSnapshot(int maxLength)
        {
            lock(lockObject)
            {
                if(tasks.Count > maxLength)
                    return null;
                return new OldWaitingCounterSnapshot() {Tasks = tasks.ToArray(), NotCountedNewTasks = new Dictionary<string, long>(notCountedNewTasks)};
            }
        }

        public void NewMetainformationAvailable(TaskMetaInformation[] metas, long now)
        {
            lock(lockObject)
            {
                var borderTicks = now - watchInterval;

                notCountedNewTasks.DeleteWhere(pair =>
                    {
                        if(pair.Value < borderTicks)
                        {
                            CountTask(pair.Key, true);
                            return true;
                        }
                        return false;
                    });
                //note metas sorted by LastModificationTicks
                foreach(var taskMetaInformation in metas)
                {
                    var taskId = taskMetaInformation.Id;
                    var minimalStartTicks = taskMetaInformation.MinimalStartTicks;
                    if(minimalStartTicks >= borderTicks)
                    {
                        if(IsWaitingState(taskMetaInformation))
                        {
                            notCountedNewTasks[taskId] = minimalStartTicks;
                            CountTask(taskId, false);
                        }
                        else
                        {
                            CountTask(taskId, false);
                            notCountedNewTasks.Remove(taskId);
                        }
                    }
                    else
                        CountTask(taskId, IsWaitingState(taskMetaInformation));
                }
                SetValue(tasks.Count);
            }
        }

        public string[] GetOldWaitingTaskIds()
        {
            lock(lockObject)
            {
                return tasks.ToArray();
            }
        }

        private void CountTask(string taskId, bool isWaitingState)
        {
            if(tasks.Contains(taskId))
            {
                if(!isWaitingState)
                {
                    if(tasks.Remove(taskId))
                        lostLogger.InfoFormat("Task {0} is removed", taskId);
                }
            }
            else
            {
                if(isWaitingState)
                {
                    if(tasks.Add(taskId))
                        lostLogger.InfoFormat("Task {0} is added", taskId);
                }
            }
        }

        public Status GetStatus()
        {
            lock(lockObject)
                return new Status()
                    {
                        NotCountedNewTasksCount = notCountedNewTasks.Count
                    };
        }

        private void SetValue(int count)
        {
            Interlocked.Exchange(ref value, count);
        }

        public long GetValue()
        {
            LogTasks();
            return Interlocked.Read(ref value);
        }

        private void LogTasks()
        {
            if(DateTime.UtcNow - lastLogTasksDateTime <= TimeSpan.FromMinutes(1))
                return;
            lock(lockObject)
            {
                var taskIds = tasks.ToArray();
                if(taskIds.Length > 0)
                {
                    logger.WarnFormat("Probably lost tasks: {0}", string.Join(",", taskIds.Take(100)));
                    lastLogTasksDateTime = DateTime.UtcNow;
                }
            }
        }

        private DateTime lastLogTasksDateTime = DateTime.MinValue;
        private static readonly ILog logger = Log.For("NewEventsCounter");
        private static readonly ILog lostLogger = Log.For("LostTasks");

        private readonly long watchInterval;
        private volatile Dictionary<string, long> notCountedNewTasks;
        private readonly HashSet<string> tasks = new HashSet<string>();
        private readonly object lockObject = new object();
        private long value;

        public class Status
        {
            public int NotCountedNewTasksCount { get; set; }
        }
    }
}