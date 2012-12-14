using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using SKBKontur.Catalogue.Core.SynchronizationStorage.LocalStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;

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
                var updatedTasksIds = handleTasksMetaStorage.GetAllTasksInStatesFromTicks(lastTicks,
                                                                                          TaskState.Canceled,
                                                                                          TaskState.Fatal,
                                                                                          TaskState.Finished,
                                                                                          TaskState.InProcess,
                                                                                          TaskState.New,
                                                                                          TaskState.Unknown,
                                                                                          TaskState.WaitingForRerun,
                                                                                          TaskState.WaitingForRerunAfterError).ToArray();
                // note не продолбаем таски?
                if(updatedTasksIds.Count() == 0)
                    return;
                var updatedTasksMetas = updatedTasksIds.Select(x => handleTasksMetaStorage.GetMeta(x));
                lastTicks = updatedTasksMetas.Max(x => x.MinimalStartTicks) + 1;
                var hs = new Dictionary<string, TaskMetaInformationBusinessObjectWrap>();
                foreach(var taskId in updatedTasksIds)
                {
                    if(hs.ContainsKey(taskId))
                        hs[taskId].Info = handleTasksMetaStorage.GetMeta(taskId);
                    else
                    {
                        hs.Add(taskId, new TaskMetaInformationBusinessObjectWrap
                            {
                                Id = taskId,
                                ScopeId = taskId,
                                Info = handleTasksMetaStorage.GetMeta(taskId)
                            });
                    }
                }
                localStorage.Write(hs.Select(x => x.Value).ToArray());
            }
        }

        private long lastTicks;

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ILocalStorage localStorage;

        private readonly object lockObject = new object();
    }
}