using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using MoreLinq;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    internal class TasksCache : ITasksCache
    {
        public IEnumerable<Tuple<string, ColumnInfo>> PassThroughtCache(IEnumerable<Tuple<string, ColumnInfo>> enumerable)
        {
            var snapshot = cache.Select(pair => new Tuple<string, ColumnInfo>(pair.Key, pair.Value)).ToArray();
            return enumerable.Pipe(Add).Concat(snapshot);
        }

        public bool Remove(string taskId)
        {
            ColumnInfo info;
            return cache.TryRemove(taskId, out info);
        }

        private void Add(Tuple<string, ColumnInfo> element)
        {
            cache.AddOrUpdate(element.Item1, element.Item2, (s, info) => element.Item2);
        }

        private readonly ConcurrentDictionary<string, ColumnInfo> cache = new ConcurrentDictionary<string, ColumnInfo>();
    }
}