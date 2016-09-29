using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Repositories.BlobStorages;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteQueue.Cassandra.Entities
{
    public class TaskMetaInformation
    {
        public TaskMetaInformation([NotNull] string name, [NotNull] string id)
        {
            Name = name;
            Id = id;
        }

        [NotNull]
        public string Name { get; private set; }

        [NotNull]
        public string Id { get; private set; }

        [CanBeNull]
        public BlobId TaskDataId { get; set; }

        [CanBeNull]
        public List<TimeGuid> TaskExceptionInfoIds { get; set; }

        public long Ticks { get; set; }
        public long MinimalStartTicks { get; set; }
        public long? StartExecutingTicks { get; set; }
        public long? FinishExecutingTicks { get; set; }
        public long? LastModificationTicks { get; set; }
        public long? TtlTicks { get; private set; }
        public long? ExpirationTimestampTicks { get; private set; }
        public TaskState State { get; set; }
        public int Attempts { get; set; }
        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }
        public string TraceId { get; set; }
        public bool TraceIsActive { get; set; }

        internal bool IsTimeBased()
        {
            TimeGuid timeGuid;
            return TimeGuid.TryParse(Id, out timeGuid);
        }

        [NotNull]
        internal BlobId GetTaskDataId()
        {
            if(TaskDataId == null)
                throw new InvalidProgramStateException(string.Format("TaskDataId is not set for: {0}", this));
            return TaskDataId;
        }

        [NotNull]
        internal List<TimeGuid> GetTaskExceptionInfoIds()
        {
            return TaskExceptionInfoIds ?? new List<TimeGuid>();
        }

        [NotNull]
        internal List<TimeGuid> AddExceptionInfoId([NotNull] TimeGuid newExceptionInfoId, [CanBeNull] out TimeGuid oldExceptionInfoId)
        {
            var taskExceptionInfoIds = GetTaskExceptionInfoIds();
            if(taskExceptionInfoIds.Count < TaskExceptionIfoIdsLimit)
                oldExceptionInfoId = null;
            else
            {
                const int oldExceptionInfoIdIndex = TaskExceptionIfoIdsLimit / 2;
                oldExceptionInfoId = taskExceptionInfoIds[oldExceptionInfoIdIndex];
                taskExceptionInfoIds = taskExceptionInfoIds.Take(oldExceptionInfoIdIndex).Concat(taskExceptionInfoIds.Skip(oldExceptionInfoIdIndex + 1)).ToList();
            }
            taskExceptionInfoIds.Add(newExceptionInfoId);
            return taskExceptionInfoIds;
        }

        internal TimeSpan? GetTtl()
        {
            return TtlTicks.HasValue ? TimeSpan.FromTicks(TtlTicks.Value) : (TimeSpan?)null;
        }

        [CanBeNull]
        internal Timestamp GetExpirationTimestamp()
        {
            return ExpirationTimestampTicks.HasValue ? new Timestamp(ExpirationTimestampTicks.Value) : null;
        }

        public void SetMinimalStartTicks([NotNull] Timestamp newMinimalStartTimestamp, TimeSpan ttl)
        {
            var now = Timestamp.Now;
            var expirationTimestamp = Max(now, newMinimalStartTimestamp) + ttl;
            TtlTicks = (expirationTimestamp - now).Ticks;
            ExpirationTimestampTicks = expirationTimestamp.Ticks;
            MinimalStartTicks = newMinimalStartTimestamp.Ticks;
        }

        internal bool NeedTtlProlongation([CanBeNull] Timestamp oldExpirationTimestamp)
        {
            if(oldExpirationTimestamp == null || !TtlTicks.HasValue)
                return true;
            var halfOfTtl = TimeSpan.FromTicks((TtlTicks.Value + 1) / 2);
            return Max(Timestamp.Now, GetMinimalStartTimestamp()) + halfOfTtl > oldExpirationTimestamp;
        }

        [NotNull]
        private Timestamp GetMinimalStartTimestamp()
        {
            if(MinimalStartTicks >= Timestamp.MinValue.Ticks && MinimalStartTicks <= Timestamp.MaxValue.Ticks)
                return new Timestamp(MinimalStartTicks);
            return Timestamp.MinValue;
        }

        [NotNull]
        private static Timestamp Max([NotNull] Timestamp t1, [NotNull] Timestamp t2)
        {
            return t1 > t2 ? t1 : t2;
        }

        public override string ToString()
        {
            string taskExceptionInfoIds;
            if(TaskExceptionInfoIds == null)
                taskExceptionInfoIds = "NONE";
            else if(TaskExceptionInfoIds.Count == 1)
                taskExceptionInfoIds = string.Format("SingleExceptionId = {0}", TaskExceptionInfoIds.Single());
            else
                taskExceptionInfoIds = string.Format("FirstExceptionId = {0}, LastExceptionId = {1}, Count = {2}", TaskExceptionInfoIds.First(), TaskExceptionInfoIds.Last(), TaskExceptionInfoIds.Count);
            var minimalStartTicks = TicksToString(MinimalStartTicks);
            var ttl = TtlTicks.HasValue ? new TimeSpan(TtlTicks.Value).ToString() : "NONE";
            var expirationTimestamp = ExpirationTimestampTicks.HasValue ? TicksToString(ExpirationTimestampTicks.Value) : "NONE";
            return string.Format("[Name: {0}, Id: {1}, State: {2}, Attempts: {3}, MinimalStartTicks: {4}, ParentTaskId: {5}, TaskGroupLock: {6}, TraceId: {7}, TaskDataId: {8}, TaskExceptionInfoIds: {9} Ttl: {10}, ExpirationTimestamp: {11}]",
                                 Name, Id, State, Attempts, minimalStartTicks, ParentTaskId, TaskGroupLock, TraceId, TaskDataId, taskExceptionInfoIds, ttl, expirationTimestamp);
        }

        private static string TicksToString(long ticks)
        {
            return ticks >= Timestamp.MinValue.Ticks && ticks <= Timestamp.MaxValue.Ticks ? new Timestamp(ticks).ToString() : ticks.ToString();
        }

        public const int TaskExceptionIfoIdsLimit = 201;
    }
}