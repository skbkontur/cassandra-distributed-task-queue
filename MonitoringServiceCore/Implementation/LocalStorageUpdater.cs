using System;
using System.Collections.Generic;
using System.Linq;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Core.SynchronizationStorage.EventDevourers;
using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler;

using log4net;

using MTaskState = SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives.TaskState;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class LocalStorageUpdater : ILocalStorageUpdater
    {
        public LocalStorageUpdater
            (IHandleTasksMetaStorage handleTasksMetaStorage,
             IEventLogRepository eventLogRepository,
             ILocalStorage localStorage,
             IGlobalTime globalTime,
             IThreadUpdateStartingTicksCache startingTicksCache,
             IEventCache eventCache,
             ICassandraClusterSettings cassandraClusterSettings)
        {
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.eventLogRepository = eventLogRepository;
            this.localStorage = localStorage;
            this.globalTime = globalTime;
            this.startingTicksCache = startingTicksCache;
            this.eventCache = eventCache;
            this.cassandraClusterSettings = cassandraClusterSettings;
        }

        public void Update()
        {
            var guid = Guid.NewGuid().ToString();
            try
            {
                startingTicksCache.Add(guid, GetStartTime());
                eventCache.RemoveEvents(startingTicksCache.GetMinimum());
                var lastTicks = globalTime.GetNowTicks();
                UpdateLocalStorage(eventLogRepository.GetEvents(localStorage.GetLastUpdateTime<MonitoringTaskMetadata>()));
                UpdateLocalStorageTicks(lastTicks);
            }
            finally
            {
                startingTicksCache.Remove(guid);
            }
        }

        public void ClearCache()
        {
            eventCache.Clear();
        }

        public void RecalculateInProcess()
        {
            var metadatas = localStorage.Search<MonitoringTaskMetadata>(
                x => x.State == MTaskState.New ||
                     x.State == MTaskState.InProcess ||
                     x.State == MTaskState.WaitingForRerun ||
                     x.State == MTaskState.WaitingForRerunAfterError).ToArray();
            var list = new List<MonitoringTaskMetadata>();
            foreach(var metadata in metadatas)
            {
                var meta = handleTasksMetaStorage.GetMeta(metadata.TaskId);
                MonitoringTaskMetadata newMetadata;
                if(TryConvertTaskMetaInformationToMonitoringTaskMetadata(meta, out newMetadata))
                    list.Add(newMetadata);
            }
            foreach(var batch in new SeparateOnBatchesEnumerable<MonitoringTaskMetadata>(list, 100))
                localStorage.Write(batch, false);
        }

        private void UpdateLocalStorage(IEnumerable<TaskMetaUpdatedEvent> events)
        {
            var batchCount = 0;
            foreach (var eventBatch in events.Batch(1000, Enumerable.ToArray))
            {
                logger.InfoFormat("Reading batch #{0} with {1} events", batchCount++, eventBatch.Length);

                var uniqueEventBatch = eventBatch.GroupBy(x => x.TaskId).Select(x => x.MaxBy(y => y.Ticks)).ToArray();

                var taskMetas = handleTasksMetaStorage.GetMetas(uniqueEventBatch.Select(x => x.TaskId).ToArray()).ToDictionary(x => x.Id);
                if(uniqueEventBatch.Length > taskMetas.Count)
                    logger.WarnFormat("Lost {0} task metas", uniqueEventBatch.Length - taskMetas.Count);

                var list = new List<MonitoringTaskMetadata>();

                foreach(var taskEvent in uniqueEventBatch)
                {
                    if(eventCache.Contains(taskEvent))
                        continue;

                    TaskMetaInformation taskMeta;
                    if(!taskMetas.TryGetValue(taskEvent.TaskId, out taskMeta))
                    {
                        logger.WarnFormat("Cannot read meta for '{0}'", taskEvent.TaskId);
                        continue;
                    }

                    if(taskMeta.LastModificationTicks == null)
                    {
                        logger.WarnFormat("TaskMeta with id='{0}' have LastModificationTicks==[null]", taskEvent.TaskId);
                        continue;
                    }

                    if(taskEvent.Ticks > taskMeta.LastModificationTicks)
                    {
                        logger.InfoFormat("TaskMeta with id='{0}' have too old LastModificationTicks", taskEvent.TaskId);
                        continue;
                    }

                    MonitoringTaskMetadata metadata;
                    if(!TryConvertTaskMetaInformationToMonitoringTaskMetadata(taskMeta, out metadata))
                    {
                        logger.WarnFormat("Error while index metadata for task '{0}'", taskMeta.Id);
                        continue;
                    }

                    list.Add(metadata);
                    eventCache.AddEvents(new[] {taskEvent});
                }

                foreach(var batch in list.Batch(100, Enumerable.ToArray))
                    localStorage.Write(batch, false);
                logger.InfoFormat("Wrote {0} rows in sql", list.Count);

                UpdateLocalStorageTicks(eventBatch.Last().Ticks);
            }
        }

        private void UpdateLocalStorageTicks(long lastTicks)
        {
            lock (lockObject)
            {
                if (localStorage.GetLastUpdateTime<MonitoringTaskMetadata>() < lastTicks)
                    localStorage.SetLastUpdateTime<MonitoringTaskMetadata>(lastTicks);
            }
        }

        private long GetStartTime()
        {
            var maxCassandraWriteTimeout = cassandraClusterSettings.Timeout * 10000 * cassandraClusterSettings.Attempts;
            return localStorage.GetLastUpdateTime<MonitoringTaskMetadata>() - maxCassandraWriteTimeout;
        }

        private bool TryConvertTaskMetaInformationToMonitoringTaskMetadata(TaskMetaInformation info, out MonitoringTaskMetadata taskMetadata)
        {
            if(info == null)
            {
                taskMetadata = new MonitoringTaskMetadata();
                logger.Error("MetaInformation null");
                return false;
            }
            taskMetadata = new MonitoringTaskMetadata
                {
                    Name = info.Name,
                    TaskId = info.Id,
                    Ticks = new DateTime(info.Ticks),
                    MinimalStartTicks = new DateTime(info.MinimalStartTicks),
                    StartExecutingTicks = NullableTickToNullableDateTime(info.StartExecutingTicks),
                    FinishExecutingTicks = NullableTickToNullableDateTime(info.FinishExecutingTicks),
                    LastModificationDateTime = NullableTickToNullableDateTime(info.LastModificationTicks),
                    State = default(MTaskState),
                    Attempts = info.Attempts,
                    ParentTaskId = info.ParentTaskId,
                    TaskGroupLock = info.TaskGroupLock,
                };
            MTaskState mtaskState;
            if(!Enum.TryParse(info.State.ToString(), true, out mtaskState))
            {
                logger.ErrorFormat("Не смог сконвертировать TaskState(RemouteTaskQueue) к TaskState(MonitoringDataTypes). TaskId: {0}", taskMetadata.TaskId); // todo написать нормальное сообщение
                return false;
            }
            taskMetadata.State = mtaskState;
            return true;
        }

        private static DateTime? NullableTickToNullableDateTime(long? value)
        {
            return value.HasValue ? (DateTime?)new DateTime(value.Value) : null;
        }

        private readonly ILog logger = LogManager.GetLogger(typeof(MonitoringTask));
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly IEventLogRepository eventLogRepository;
        private readonly ILocalStorage localStorage;
        private readonly IGlobalTime globalTime;
        private readonly IThreadUpdateStartingTicksCache startingTicksCache;
        private readonly IEventCache eventCache;
        private readonly ICassandraClusterSettings cassandraClusterSettings;
        private readonly object lockObject = new object();
    }
}