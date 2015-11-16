using System.Collections.Generic;
using System.Linq;
using System.Threading;

using log4net;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.Utils;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.NewEventsCounters
{
    public class NewTasksCounter
    {
        public NewTasksCounter(long watchInterval)
        {
            this.watchInterval = watchInterval;
            value = 0;
        }

        public NewTasksCounter()
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
                tasks.Clear();
                SetValue(0);
            }
        }

        public void LoadSnapshot(NewEventsCounterSnapshot snapshot)
        {
            if(snapshot == null || snapshot.Tasks == null)
            {
                logger.LogWarnFormat("Snapshot is empty");
                return;
            }
            lock(lockObject)
            {
                tasks.Clear();
                foreach(var task in snapshot.Tasks)
                    tasks.Add(task);
                SetValue(tasks.Count);
            }
        }

        public NewEventsCounterSnapshot GetSnapshot(int maxLength)
        {
            lock(lockObject)
            {
                if(tasks.Count > maxLength)
                    return null;
                return new NewEventsCounterSnapshot() {Tasks = tasks.ToArray()};
            }
        }

        public void NewMetainformationAvailable(TaskMetaInformation[] metas, long now)
        {
            lock(lockObject)
            {
                var borderTicks = now - watchInterval;
                foreach(var taskMetaInformation in metas)
                {
                    var isWaitingState = IsWaitingState(taskMetaInformation);
                    var taskId = taskMetaInformation.Id;
                    var minimalStartTicks = taskMetaInformation.MinimalStartTicks;
                    if(minimalStartTicks < borderTicks)
                        CountTask(taskId, isWaitingState);
                    else
                    {
                        if(isWaitingState)
                            notCountedNewTasks[taskId] = minimalStartTicks;
                        else
                        {
                            CountTask(taskId, false);
                            notCountedNewTasks.Remove(taskId);
                        }
                    }
                }
                notCountedNewTasks.DeleteWhere(pair =>
                    {
                        if(pair.Value < borderTicks)
                        {
                            CountTask(pair.Key, true);
                            return true;
                        }
                        return false;
                    });
                SetValue(tasks.Count);
            }
        }

        private void CountTask(string taskId, bool isWaitingState)
        {
            if(tasks.Contains(taskId))
            {
                if(!isWaitingState)
                    tasks.Remove(taskId);
            }
            else
            {
                if(isWaitingState)
                    tasks.Add(taskId);
            }
        }

        private void SetValue(int count)
        {
            Interlocked.Exchange(ref value, count);
        }

        public long GetValue()
        {
            return Interlocked.Read(ref value);
        }

        private static readonly ILog logger = Log.For("NewEventsCounter");

        private readonly long watchInterval;
        private readonly Dictionary<string, long> notCountedNewTasks = new Dictionary<string, long>();
        private readonly HashSet<string> tasks = new HashSet<string>();
        private readonly object lockObject = new object();
        private long value;
    }
}