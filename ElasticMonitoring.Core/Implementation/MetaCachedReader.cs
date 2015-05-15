using System.Collections.Generic;
using System.Linq;

using GroboContainer.Infection;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class MetaCachedReader : IMetaCachedReader
    {
        public MetaCachedReader(IHandleTasksMetaStorage handleTasksMetaStorage, long cacheTimeoutTicks)
        {
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.cacheTimeoutTicks = cacheTimeoutTicks;
        }

        [ContainerConstructor]
        public MetaCachedReader(IHandleTasksMetaStorage handleTasksMetaStorage)
            : this(handleTasksMetaStorage, TaskIndexSettings.MetaCacheInterval.Ticks)
        {
        }

        private TaskMetaInformation[] ReadRaw(string[] ids)
        {
            return handleTasksMetaStorage.GetMetas(ids);
        }

        private void CollectGarbage(long nowTicks)
        {
            lock(stateLock)
                cache.DeleteWhere(pair => nowTicks - pair.Value.LastModificationTicks.Value > cacheTimeoutTicks);
        }

        public TaskMetaInformation[] ReadActualMetasQuiet(TaskMetaUpdatedEvent[] events, long nowTicks)
        {
            var result = new TaskMetaInformation[events.Length];
            var notCached = ReadFromCache(events, result);

            ReadFromStorage(notCached, events, result);

            CollectGarbage(nowTicks);
            return result.ToArray();
        }

        private void ReadFromStorage(HashSet<string> notCached, TaskMetaUpdatedEvent[] events, TaskMetaInformation[] result)
        {
            var rawMetas = ReadRaw(notCached.ToArray());
            var taskMetaInformations = rawMetas.ToDictionary(information => information.Id);
            lock(stateLock)
            {
                for(var i = 0; i < events.Length; i++)
                {
                    TaskMetaInformation value;
                    if(taskMetaInformations.TryGetValue(events[i].TaskId, out value))
                    {
                        var cachedMeta = MergeWithCache(value);
                        if(cachedMeta.LastModificationTicks.Value >= events[i].Ticks)
                            result[i] = cachedMeta;
                    }
                }
            }
        }

        private TaskMetaInformation MergeWithCache(TaskMetaInformation metaFromStorage)
        {
            TaskMetaInformation cachedMeta;
            var id = metaFromStorage.Id;
            if(!cache.TryGetValue(id, out cachedMeta))
                cache.Add(id, cachedMeta = metaFromStorage); //not in cache
            else
            {
                if(cachedMeta.LastModificationTicks.Value < metaFromStorage.LastModificationTicks.Value)
                    cache[id] = (cachedMeta = metaFromStorage); //note update in cache
            }
            return cachedMeta;
        }

        private HashSet<string> ReadFromCache(TaskMetaUpdatedEvent[] events, TaskMetaInformation[] result)
        {
            lock(stateLock)
            {
                var notCached = new HashSet<string>();
                for(var index = 0; index < events.Length; index++)
                {
                    var taskMetaUpdatedEvent = events[index];
                    var id = taskMetaUpdatedEvent.TaskId;
                    TaskMetaInformation cachedMeta;
                    if(cache.TryGetValue(id, out cachedMeta))
                    {
                        if(cachedMeta.LastModificationTicks.Value >= taskMetaUpdatedEvent.Ticks)
                            result[index] = cachedMeta;
                        else
                        {
                            cache.Remove(id);
                            notCached.Add(id);
                        }
                    }
                    else
                        notCached.Add(id);
                }
                return notCached;
            }
        }

        private readonly object stateLock = new object();

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;

        private readonly long cacheTimeoutTicks;
        private readonly Dictionary<string, TaskMetaInformation> cache = new Dictionary<string, TaskMetaInformation>();
    }
}