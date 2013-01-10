using System;
using System.Collections.Generic;
using System.Linq;

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
                UpdateLocalStorage(eventLogRepository.GetEvents(localStorage.GetLastUpdateTime<MonitoringTaskMetadata>()).ToArray());
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

        private void UpdateLocalStorage(TaskMetaUpdatedEvent [] events)
        {
            foreach(var eventBatch in new SeparateOnBatchesEnumerable<TaskMetaUpdatedEvent>(events, 300))
            {
                var updateTime = globalTime.GetNowTicks();
                var hs = new Dictionary<string, MonitoringTaskMetadata>();
                foreach(var taskEvent in eventBatch)
                {
                    if(eventCache.Contains(taskEvent))
                        continue;
                    var taskMeta = handleTasksMetaStorage.GetMeta(taskEvent.TaskId);
                    MonitoringTaskMetadata metadata;
                    if(TryConvertTaskMetaInformationToMonitoringTaskMetadata(taskMeta, out metadata))
                    {
                        if(hs.ContainsKey(metadata.Id))
                        {
                            if(metadata.Ticks > hs[metadata.Id].Ticks) //note это условие вроде всегда выполн€етс€
                                hs[metadata.Id] = metadata;
                        }
                        else
                            hs.Add(metadata.Id, metadata);
                    }
                }
                var forUpdateWithoutR = hs.Select(x => x.Value).ToArray();
                if(hs.Count != 0)
                {
                    foreach(var batch in new SeparateOnBatchesEnumerable<MonitoringTaskMetadata>(forUpdateWithoutR, 100))
                        localStorage.Write(batch);
                }
                hs.Clear();
                localStorage.SetLastUpdateTime<MonitoringTaskMetadata>(updateTime);
            }
            eventCache.AddEvents(events);
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
                    State = default(MTaskState),
                    Attempts = info.Attempts,
                    ParentTaskId = info.ParentTaskId
                };
            MTaskState mtaskState;
            if(!Enum.TryParse(info.State.ToString(), true, out mtaskState))
            {
                logger.ErrorFormat("Ќе смог сконвертировать TaskState(RemouteTaskQueue) к TaskState(MonitoringDataTypes). TaskId: {0}", taskMetadata.TaskId); // todo написать нормальное сообщение
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