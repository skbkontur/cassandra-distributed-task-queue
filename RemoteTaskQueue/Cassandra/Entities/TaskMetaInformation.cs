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
        public long? ExpirationTimestampTicks { get; private set; }
        public long? ExpirationModificationTicks { get; private set; }
        public long? ExecutionDurationTicks => FinishExecutingTicks - StartExecutingTicks;
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
            if (TaskDataId == null)
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
            if (taskExceptionInfoIds.Count < TaskExceptionIfoIdsLimit)
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
            if (!ExpirationTimestampTicks.HasValue)
                return null;
            return TimeSpan.FromTicks(Math.Max(ExpirationTimestampTicks.Value - Timestamp.Now.Ticks, TimeSpan.TicksPerSecond));
        }

        public void SetOrUpdateTtl(TimeSpan ttl)
        {
            var now = Timestamp.Now;
            var minimalStartTimestamp = GetMinimalStartTimestamp();
            if (now > minimalStartTimestamp)
            {
                ExpirationTimestampTicks = (now + ttl).Ticks;
                ExpirationModificationTicks = now.Ticks;
            }
            else
            {
                ExpirationTimestampTicks = (minimalStartTimestamp + ttl).Ticks;
                ExpirationModificationTicks = minimalStartTimestamp.Ticks;
            }
        }

        internal bool NeedTtlProlongation()
        {
            if (!ExpirationTimestampTicks.HasValue || !ExpirationModificationTicks.HasValue)
                return false;
            var halfOfGivenTtl = TimeSpan.FromTicks((ExpirationTimestampTicks.Value - ExpirationModificationTicks.Value + 1) / 2);
            return Max(Timestamp.Now, GetMinimalStartTimestamp()) + halfOfGivenTtl > GetExpirationTimestamp();
        }

        [CanBeNull]
        private Timestamp GetExpirationTimestamp()
        {
            return ExpirationTimestampTicks.HasValue ? new Timestamp(ExpirationTimestampTicks.Value) : null;
        }

        [NotNull]
        public Timestamp GetMinimalStartTimestamp()
        {
            if (MinimalStartTicks > Timestamp.MaxValue.Ticks)
                throw new InvalidProgramStateException(string.Format("Invalid MinimalStartTicks: {0}, impossible to construct Timestamp", MinimalStartTicks));
            return MinimalStartTicks < Timestamp.MinValue.Ticks ? Timestamp.MinValue : new Timestamp(MinimalStartTicks);
        }

        [NotNull]
        private static Timestamp Max([NotNull] Timestamp t1, [NotNull] Timestamp t2)
        {
            return t1 > t2 ? t1 : t2;
        }

        public override string ToString()
        {
            string taskExceptionInfoIds;
            if (TaskExceptionInfoIds == null)
                taskExceptionInfoIds = "NONE";
            else if (TaskExceptionInfoIds.Count == 1)
                taskExceptionInfoIds = string.Format("SingleExceptionId = {0}", TaskExceptionInfoIds.Single());
            else
                taskExceptionInfoIds = string.Format("FirstExceptionId = {0}, LastExceptionId = {1}, Count = {2}", TaskExceptionInfoIds.First(), TaskExceptionInfoIds.Last(), TaskExceptionInfoIds.Count);
            var minimalStartTicks = TicksToString(MinimalStartTicks);
            var expirationTimestamp = ExpirationTimestampTicks.HasValue ? TicksToString(ExpirationTimestampTicks.Value) : "NONE";
            return string.Format("[Name: {0}, Id: {1}, State: {2}, Attempts: {3}, MinimalStartTicks: {4}, ParentTaskId: {5}, TaskGroupLock: {6}, TraceId: {7}, TaskDataId: {8}, TaskExceptionInfoIds: {9}, ExpirationTimestamp: {10}]",
                                 Name, Id, State, Attempts, minimalStartTicks, ParentTaskId, TaskGroupLock, TraceId, TaskDataId, taskExceptionInfoIds, expirationTimestamp);
        }

        private static string TicksToString(long ticks)
        {
            return ticks >= Timestamp.MinValue.Ticks && ticks <= Timestamp.MaxValue.Ticks ? new Timestamp(ticks).ToString() : ticks.ToString();
        }

        public const int TaskExceptionIfoIdsLimit = 201;
    }
}