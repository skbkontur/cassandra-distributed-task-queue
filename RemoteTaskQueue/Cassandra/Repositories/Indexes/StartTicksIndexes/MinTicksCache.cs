using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class MinTicksCache : IMinTicksCache
    {
        public MinTicksCache(ITicksHolder ticksHolder)
        {
            this.ticksHolder = ticksHolder;
        }

        public void UpdateMinTicks(TaskState taskState, long ticks)
        {
            ticksCache[taskState] = Math.Max(ticks - TimeSpan.FromHours(1).Ticks, 1);
        }

        public long GetMinTicks(TaskState taskState)
        {
            if(!ticksCache.ContainsKey(taskState))
            {
                long minTicks = ticksHolder.GetMinTicks(taskState.GetCassandraName());
                if(minTicks == 0)
                    return 0;
                UpdateMinTicks(taskState, minTicks);
            }
            return ticksCache[taskState];
        }

        private readonly ITicksHolder ticksHolder;
        private readonly IDictionary<TaskState, long> ticksCache = new ConcurrentDictionary<TaskState, long>();
    }
}