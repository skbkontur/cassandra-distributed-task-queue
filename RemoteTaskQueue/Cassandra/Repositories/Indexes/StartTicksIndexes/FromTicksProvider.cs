using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class FromTicksProvider : IFromTicksProvider
    {
        public FromTicksProvider(ITicksHolder ticksHolder)
        {
            this.ticksHolder = ticksHolder;
        }

        public long? TryGetFromTicks(TaskState taskState)
        {
            var oldestLiveRecordTicks = TryGetOldestLiveRecordTicks(taskState);
            if(!oldestLiveRecordTicks.HasValue)
                return null;
            var overlapDuration = GetOverlapDuration(taskState);
            var fromTicks = oldestLiveRecordTicks.Value - overlapDuration.Ticks;
            var twoDaysSafetyBelt = (DateTime.UtcNow - TimeSpan.FromDays(2)).Ticks;
            return Math.Max(fromTicks, twoDaysSafetyBelt);
        }

        private TimeSpan GetOverlapDuration(TaskState taskState)
        {
            var utcNow = DateTime.UtcNow;
            DateTime lastBigOverlapMoment;
            if(!lastBigOverlapMomentsByTaskState.TryGetValue(taskState, out lastBigOverlapMoment) || utcNow - lastBigOverlapMoment > TimeSpan.FromMinutes(1))
            {
                lastBigOverlapMomentsByTaskState[taskState] = utcNow;
                //—ложно рассчитать математически правильный размер отката, и код постановки таски может изменитьс€,
                //что потребует изменени€ этого отката. ѕоэтому беретс€, как кажетс€, с запасом
                return TimeSpan.FromMinutes(8); // ѕротив адских затупов кассандры
            }
            return TimeSpan.FromMinutes(1); // Ўтатна€ зона нестабильности
        }

        // is called concurrently
        public void HandleTaskStateChange([NotNull] TaskMetaInformation taskMeta)
        {
            var taskState = taskMeta.State.GetCassandraName();
            ticksHolder.UpdateMinTicks(taskState, taskMeta.MinimalStartTicks);
            // todo: нужно также подвинуть влево маркер oldestLiveRecordTicks при необходимости
        }

        public void UpdateOldestLiveRecordTicks(TaskState taskState, long oldestLiveRecordTicks)
        {
            oldestLiveRecordTicksByTaskState[taskState] = Math.Max(oldestLiveRecordTicks - TimeSpan.FromMinutes(6).Ticks, 1);
        }

        private long? TryGetOldestLiveRecordTicks(TaskState taskState)
        {
            long oldestLiveRecordTicks;
            if(!oldestLiveRecordTicksByTaskState.TryGetValue(taskState, out oldestLiveRecordTicks))
            {
                oldestLiveRecordTicks = ticksHolder.GetMinTicks(taskState.GetCassandraName());
                if(oldestLiveRecordTicks == 0)
                    return null;
                UpdateOldestLiveRecordTicks(taskState, oldestLiveRecordTicks);
            }
            return oldestLiveRecordTicks;
        }

        private readonly ITicksHolder ticksHolder;
        private readonly Dictionary<TaskState, long> oldestLiveRecordTicksByTaskState = new Dictionary<TaskState, long>();
        private readonly Dictionary<TaskState, DateTime> lastBigOverlapMomentsByTaskState = new Dictionary<TaskState, DateTime>();
    }
}