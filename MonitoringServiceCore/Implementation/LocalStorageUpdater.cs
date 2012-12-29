using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

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
        public LocalStorageUpdater(IHandleTasksMetaStorage handleTasksMetaStorage, IEventLogRepository eventLogRepository, ILocalStorage localStorage, IGlobalTime globalTime)
        {
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.eventLogRepository = eventLogRepository;
            this.localStorage = localStorage;
            this.globalTime = globalTime;
        }

        public void UpdateLocalStorage()
        {
            lock(lockObject)
            {
                var updateTime = globalTime.GetNowTicks();
                var updatedTasksMetas = eventLogRepository.GetEvents(localStorage.GetLastUpdateTime<MonitoringTaskMetadata>()).Select(x => handleTasksMetaStorage.GetMeta(x.TaskId)).ToArray();
                var hs = new Dictionary<string, MonitoringTaskMetadata>();
                foreach(var taskMetas in updatedTasksMetas)
                {
                    MonitoringTaskMetadata metadata;
                    if(TryConvertTaskMetaInformationToMonitoringTaskMetadata(taskMetas, out metadata))
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
                if(hs.Count != 0)
                {
                    foreach(var batch in new SeparateOnBatchesEnumerable<MonitoringTaskMetadata>(hs.Select(x => x.Value), 500))
                        localStorage.Write(batch);
                }
                localStorage.SetLastUpdateTime<MonitoringTaskMetadata>(updateTime);
            }
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
        private readonly object lockObject = new object();
    }
}