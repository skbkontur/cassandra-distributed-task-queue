using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class FromTicksProvider : IFromTicksProvider
    {
        public FromTicksProvider(OldestLiveRecordTicksHolder oldestLiveRecordTicksHolder)
        {
            this.oldestLiveRecordTicksHolder = oldestLiveRecordTicksHolder;
        }

        public long? TryGetFromTicks(TaskState taskState)
        {
            var oldestLiveRecordTicks = oldestLiveRecordTicksHolder.TryStartReadToEndSession(taskState);
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

        public void HandleTaskStateChange([NotNull] TaskMetaInformation taskMeta)
        {
            oldestLiveRecordTicksHolder.MoveBackwardIfNecessary(taskMeta.State, taskMeta.MinimalStartTicks);
        }

        public void TryUpdateOldestLiveRecordTicks(TaskState taskState, long newOldestLiveRecordTicks)
        {
            oldestLiveRecordTicksHolder.TryMoveForward(taskState, newOldestLiveRecordTicks);
        }

        private readonly OldestLiveRecordTicksHolder oldestLiveRecordTicksHolder;
        private readonly Dictionary<TaskState, DateTime> lastBigOverlapMomentsByTaskState = new Dictionary<TaskState, DateTime>();
    }
}