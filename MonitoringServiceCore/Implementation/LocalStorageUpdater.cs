using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Sheduler;

using log4net;

using MTaskState = SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives.TaskState;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation
{
    public class LocalStorageUpdater : ILocalStorageUpdater
    {
        public LocalStorageUpdater(IHandleTasksMetaStorage handleTasksMetaStorage, ILocalStorage localStorage)
        {
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.localStorage = localStorage;
            lastTicks = 0;
        }

        public void UpdateLocalStorage()
        {
            lock(lockObject)
            {
                var updatedTasksMetas = handleTasksMetaStorage.GetAllTasksInStatesFromTicks(lastTicks,
                                                                                            TaskState.Canceled,
                                                                                            TaskState.Fatal,
                                                                                            TaskState.Finished,
                                                                                            TaskState.InProcess,
                                                                                            TaskState.New,
                                                                                            TaskState.Unknown,
                                                                                            TaskState.WaitingForRerun,
                                                                                            TaskState.WaitingForRerunAfterError).Select(x =>
                                                                                                                                            {
                                                                                                                                                var res = handleTasksMetaStorage.GetMeta(x);
                                                                                                                                                lastTicks = Math.Max(lastTicks, res.MinimalStartTicks + 1); // note не продолбаем таски?
                                                                                                                                                return res;
                                                                                                                                            }).ToArray();
                var hs = new Dictionary<string, MonitoringTaskMetadata>();
                foreach(var taskMetas in updatedTasksMetas)
                {
                    MonitoringTaskMetadata metadata;
                    if(!TryConvertTaskMetaInformationToMonitoringTaskMetadata(taskMetas, out metadata))
                        logger.Error("не смог сконвертировать TaskState(RemouteTaskQueue) к TaskState(MonitoringDataTypes)"); // todo написать нормальное сообщение
                    else
                    {
                        if(hs.ContainsKey(metadata.Id))
                            hs[metadata.Id] = metadata;
                        else
                            hs.Add(metadata.Id, metadata);
                    }
                }
                if(hs.Count != 0)
                    localStorage.Write(hs.Select(x => x.Value).ToArray());
            }
        }

        private bool TryConvertTaskMetaInformationToMonitoringTaskMetadata(TaskMetaInformation info, out MonitoringTaskMetadata taskMetadata)
        {
/*
            taskMetadata = new MonitoringTaskMetadata(
                info.Name,
                info.Id,
                info.Ticks,
                info.MinimalStartTicks,
                info.StartExecutingTicks,
                default(MTaskState),
                info.Attempts,
                info.ParentTaskId);
            */
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
                return false;
            taskMetadata.State = mtaskState;
            return true;
        }

        private long lastTicks;
        private readonly ILog logger = LogManager.GetLogger(typeof(MonitoringTask));
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ILocalStorage localStorage;

        private readonly object lockObject = new object();
    }
}