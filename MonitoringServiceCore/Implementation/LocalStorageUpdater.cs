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
                lock(lockObject)
                {
                    if(localStorage.GetLastUpdateTime<MonitoringTaskMetadata>() < lastTicks)
                        localStorage.SetLastUpdateTime<MonitoringTaskMetadata>(lastTicks);
                }
            }
            finally
            {
                startingTicksCache.Remove(guid);
            }
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
                localStorage.Write(batch);
        }

        private void UpdateLocalStorage(IEnumerable<TaskMetaUpdatedEvent> events)
        {
            var batchCount = 0;
            foreach(var eventBatch in events.Batch(1000, Enumerable.ToArray))
            {
                logger.InfoFormat("Reading batch #{0}", batchCount++);

                var uniqueEventBatch = eventBatch.DistinctBy(x => x.TaskId).ToArray();

                var taskMetas = handleTasksMetaStorage.GetMetas(uniqueEventBatch.Select(x => x.TaskId).ToArray()).ToDictionary(x => x.Id);

                var list = new List<MonitoringTaskMetadata>();

                foreach (var taskEvent in uniqueEventBatch)
                {
                    if(eventCache.Contains(taskEvent)) 
                        continue;

                    TaskMetaInformation taskMeta;
                    if(!taskMetas.TryGetValue(taskEvent.TaskId, out taskMeta))
                    {
                        logger.WarnFormat("Cannot read meta for '{0}'", taskEvent.TaskId);
                        continue;
                    }
                    if (taskEvent.Ticks <= taskMeta.LastModificationTicks)
                    {
                        MonitoringTaskMetadata metadata;
                        
                        if(TryConvertTaskMetaInformationToMonitoringTaskMetadata(taskMeta, out metadata))
                            list.Add(metadata); 
                        else 
                            logger.WarnFormat("Error while index metadata for task '{0}'", taskMeta.Id);
                    }
                }

                foreach(var batch in list.Batch(100, Enumerable.ToArray))
                    localStorage.Write(batch);
                
                eventCache.AddEvents(eventBatch);
                localStorage.SetLastUpdateTime<MonitoringTaskMetadata>(eventBatch.Last().Ticks);
            }
            
        }

        private long GetTicks(DateTime? dateTime)
        {
            if(dateTime == null) return 0;
            return dateTime.Value.Ticks;
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
                    StartExecutingTicks = info.StartExecutingTicks.HasValue ? (DateTime?)new DateTime(info.StartExecutingTicks.Value) : null,
                    FinishExecutingTicks = info.FinishExecutingTicks.HasValue ? (DateTime?)new DateTime(info.FinishExecutingTicks.Value) : null,
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