using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        public void CollectGarbage(long nowTicks)
        {
            lock(stateLock)
            {
                cache.DeleteWhere(pair => nowTicks - pair.Value.LastModificationTicks.Value > cacheTimeoutTicks);
                UpdateCount();
            }
        }

        public TaskMetaInformation[] ReadActualMetasQuiet(TaskMetaUpdatedEvent[] events, long nowTicks)
        {
            var result = new TaskMetaInformation[events.Length];
            var notCached = ReadFromCache(events, result);

            ReadFromStorage(notCached, events, result);
            return result.ToArray();
        }

        private void ReadFromStorage(HashSet<string> notCached, TaskMetaUpdatedEvent[] events, TaskMetaInformation[] result)
        {
            var taskMetas = handleTasksMetaStorage.GetMetas(notCached.ToArray());
            lock(stateLock)
            {
                for(var i = 0; i < events.Length; i++)
                {
                    TaskMetaInformation taskMeta;
                    if(taskMetas.TryGetValue(events[i].TaskId, out taskMeta))
                    {
                        var cachedMeta = MergeWithCache(taskMeta);
                        if(cachedMeta.LastModificationTicks.Value >= events[i].Ticks)
                            result[i] = cachedMeta;
                    }
                }
            }
        }

        private void UpdateCount()
        {
            Interlocked.Exchange(ref count, cache.Count);
        }

        public long UnsafeGetCount()
        {
            return Interlocked.Read(ref count);
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
            UpdateCount();

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
                    UpdateCount();
                }
                return notCached;
            }
        }

        private long count;

        private readonly object stateLock = new object();

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;

        private readonly long cacheTimeoutTicks;
        private readonly Dictionary<string, TaskMetaInformation> cache = new Dictionary<string, TaskMetaInformation>();
    }
}