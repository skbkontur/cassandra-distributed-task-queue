using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using log4net;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public class MetaProviderImpl
    {
        public MetaProviderImpl(long maxEventLifetimeTicks, int maxBatch, long startTicks, IEventLogRepository eventLogRepository, IHandleTasksMetaStorage handleTasksMetaStorage, string loggerName)
        {
            logger = LogManager.GetLogger(loggerName);
            this.maxEventLifetimeTicks = maxEventLifetimeTicks;
            this.eventLogRepository = eventLogRepository;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.loggerName = loggerName;
            this.maxBatch = maxBatch;
            unstableZoneTicks = eventLogRepository.UnstableZoneLength.Ticks;
            cacheEventTicks = unstableZoneTicks * 2;
            InternalRestart(startTicks);
        }

        public void Restart(long fromTicksUtc)
        {
            InternalRestart(fromTicksUtc);
        }

        public MetaProviderSnapshot GetSnapshotOrNull(int maxLength)
        {
            lock(dataLock)
            {
                if(notReadEvents.Count > maxLength || readEvents.Count > maxLength) //todo ?? different limits
                    return null;
                return new MetaProviderSnapshot(lastUpdateTicks, 0, notReadEvents, readEvents);
            }
        }

        public void LoadSnapshot(MetaProviderSnapshot snapshot, long notEarlierTicksUtc)
        {
            if(snapshot.LastUpdateTicks < notEarlierTicksUtc)
            {
                logger.LogWarnFormat(loggerName, "Snapshot time={0} is too old. Restarting MetaProvider from {1}",
                                     DateTimeFormatter.FormatWithMsAndTicks(snapshot.LastUpdateTicks),
                                     DateTimeFormatter.FormatWithMsAndTicks(notEarlierTicksUtc));
                Restart(notEarlierTicksUtc);
            }
            else
            {
                logger.LogInfoFormat(loggerName, "Loading Snapshot. lastUpdate={0} startTime={1}. NotEarlier={2}",
                                     DateTimeFormatter.FormatWithMsAndTicks(snapshot.LastUpdateTicks),
                                     DateTimeFormatter.FormatWithMsAndTicks(snapshot.StartTicks),
                                     DateTimeFormatter.FormatWithMsAndTicks(notEarlierTicksUtc));
                lock(updateLock)
                    lock(dataLock)
                    {
                        cancel = false;
                        lastUpdateTicks = snapshot.LastUpdateTicks;
                        //startTicks = snapshot.StartTicks;
                        notReadEvents = snapshot.NotReadEvents ?? new Dictionary<string, long>();
                        readEvents = snapshot.ReadEvents ?? new Dictionary<string, long>();
                    }
            }
        }

        private void DieIfCancelled()
        {
            if(cancel)
                throw new NotSupportedException("Operation cancelled - need reset");
        }

        public void CancelLoading()
        {
            cancel = true;
        }

        public void LoadMetas(long toTicks, IMetaConsumer[] consumers)
        {
            DieIfCancelled();
            lock(updateLock)
            {
                var lastTicks = GetLastTicks();
                var events = eventLogRepository.GetEvents(lastTicks, maxBatch);

                var w = Stopwatch.StartNew();
                var notEmpty = false;
                events.Where(IsEventOk).Batch(maxBatch).ForEach(enumerable =>
                    {
                        if(cancel)
                        {
                            logger.LogWarnFormat(loggerName, "Loading is cancelled.");
                            return;
                        }
                        notEmpty = true;
                        ProcessEventsBatch(w, enumerable, toTicks, consumers);
                    });

                if(!notEmpty)
                    ProcessEventsBatch(w, new TaskMetaUpdatedEvent[0], toTicks, consumers);

                lock(dataLock)
                    lastUpdateTicks = toTicks;
            }
        }

        private static bool IsEventOk(TaskMetaUpdatedEvent @event)
        {
            return @event.TaskId != null;
        }

        private void InternalRestart(long fromTicksUtc)
        {
            lock(dataLock)
            {
                cancel = false;
                logger.LogInfoFormat(loggerName, "Restarting to time={0}", DateTimeFormatter.FormatWithMsAndTicks(fromTicksUtc));
                //startTicks = fromTicksUtc;
                lastUpdateTicks = fromTicksUtc;
                notReadEvents = new Dictionary<string, long>();
                readEvents = new Dictionary<string, long>();
            }
        }

        private static void NotifyConsumers(IMetaConsumer[] consumers, TaskMetaInformation[] metas, long readTicks)
        {
            foreach(var metaConsumer in consumers)
                metaConsumer.ProcessMetas(metas, readTicks);
        }

        private void CollectUnprocessedEventsGarbage(Dictionary<string, long> events, long nowTicks)
        {
            var idsToRemove = new HashSet<string>();
            foreach(var pair in events)
            {
                if(pair.Value + maxEventLifetimeTicks < nowTicks)
                    idsToRemove.Add(pair.Key);
            }
            DeleteByKeys(events, idsToRemove);
        }

        private void CollectAlreadyReadEventsGarbage(Dictionary<string, long> events, long nowTicks)
        {
            var idsToRemove = new HashSet<string>();
            foreach(var pair in events)
            {
                if(pair.Value + cacheEventTicks < nowTicks)
                    idsToRemove.Add(pair.Key);
            }
            DeleteByKeys(events, idsToRemove);
        }

        private static void DeleteByKeys(Dictionary<string, long> readEvents, IEnumerable<string> idsToRemove)
        {
            foreach(var id in idsToRemove)
                readEvents.Remove(id);
        }

        private void ProcessEventsBatch(Stopwatch w, IEnumerable<TaskMetaUpdatedEvent> events, long nowTicks, IMetaConsumer[] consumers)
        {
            var elapesed = w.Elapsed;
            Dictionary<string, long> readEventsCopy;
            Dictionary<string, long> notReadEventsCopy;
            lock(dataLock)
            {
                readEventsCopy = new Dictionary<string, long>(readEvents);
                notReadEventsCopy = new Dictionary<string, long>(notReadEvents);
            }

            CollectAlreadyReadEventsGarbage(readEventsCopy, nowTicks);
            CollectUnprocessedEventsGarbage(notReadEventsCopy, nowTicks);
            long maxTicks = 0;
            foreach(var @event in events)
            {
                if(cancel)
                    return;
                var taskId = @event.TaskId;
                maxTicks = Math.Max(maxTicks, @event.Ticks);
                UpdateMaxTicks(notReadEventsCopy, taskId, @event.Ticks);
            }
            if(elapesed > MetaProviderSettings.SlowCalculationIntervalMs)
            {
                logger.LogInfoFormat(loggerName, "Update is slow. Time elapsed={0}. Read Events before {1}. Now={2}",
                                     DateTimeFormatter.FormatTimeSpan(elapesed),
                                     DateTimeFormatter.FormatWithMsAndTicks(maxTicks),
                                     DateTimeFormatter.FormatWithMsAndTicks(nowTicks));
            }
            RemoveAlreadyReadEvents(notReadEventsCopy, readEventsCopy);
            
            foreach (var readEvent in notReadEventsCopy)
            {
                LogManager.GetLogger("ZZZ").LogInfoFormat("ZZZ", "Process id={0} ticks={1}", readEvent.Key, readEvent.Value);
            }

            //todo cancel read metas
            var metas = handleTasksMetaStorage.GetMetas(notReadEventsCopy.Keys.ToArray());
            var newMetas = new List<TaskMetaInformation>();
            foreach(var meta in metas)
            {
                if(meta.LastModificationTicks.HasValue)
                {
                    if(notReadEventsCopy[meta.Id] <= meta.LastModificationTicks.Value)
                    {
                        readEventsCopy[meta.Id] = meta.LastModificationTicks.Value;
                        notReadEventsCopy.Remove(meta.Id);
                        newMetas.Add(meta);
                    }
                }
            }
            NotifyConsumers(consumers, newMetas.ToArray(), nowTicks);

            lock(dataLock)
            {
                notReadEvents = notReadEventsCopy;
                readEvents = readEventsCopy;
            }
        }

        private static void RemoveAlreadyReadEvents(Dictionary<string, long> notReadEventsCopy, Dictionary<string, long> readEventsCopy)
        {
            var taskIdsToRemove = new HashSet<string>();
            foreach(var kvp in notReadEventsCopy)
            {
                var taskId = kvp.Key;
                long lastReadTicks;
                if(readEventsCopy.TryGetValue(taskId, out lastReadTicks) && lastReadTicks >= kvp.Value)
                    taskIdsToRemove.Add(taskId);
                else
                    taskIdsToRemove.Remove(taskId);
            }

            DeleteByKeys(notReadEventsCopy, taskIdsToRemove);
        }

        private static void UpdateMaxTicks(Dictionary<string, long> taskIdToMaxTicks, string taskId, long currentTicks)
        {
            long oldTicks;
            if(!taskIdToMaxTicks.TryGetValue(taskId, out oldTicks))
                taskIdToMaxTicks.Add(taskId, currentTicks);
            else
            {
                if(currentTicks > oldTicks)
                    taskIdToMaxTicks[taskId] = currentTicks;
            }
        }

        private long GetLastTicks()
        {
            long lastUpdateTicksCopy;
            lock(dataLock)
                lastUpdateTicksCopy = lastUpdateTicks;
            var lastTicks = lastUpdateTicksCopy - unstableZoneTicks;
            if(lastTicks < 0)
                return 0;
            return lastTicks;
        }

        private volatile bool cancel;

        private volatile Dictionary<string, long> notReadEvents;
        private volatile Dictionary<string, long> readEvents;

        //private long startTicks;
        private long lastUpdateTicks;

        private readonly ILog logger;

        private readonly IEventLogRepository eventLogRepository;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly string loggerName;

        private readonly object updateLock = new object();
        private readonly object dataLock = new object();

        private readonly int maxBatch;
        private readonly long maxEventLifetimeTicks;
        private readonly long cacheEventTicks;
        private readonly long unstableZoneTicks;
    }
}