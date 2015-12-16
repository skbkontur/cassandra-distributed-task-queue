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
        public List<BlobId> TaskExceptionInfoIds { get; private set; }

        public long Ticks { get; set; }
        public long MinimalStartTicks { get; set; }
        public long? StartExecutingTicks { get; set; }
        public long? FinishExecutingTicks { get; set; }
        public long? LastModificationTicks { get; set; }
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
        internal List<BlobId> GetTaskExceptionInfoIds()
        {
            return TaskExceptionInfoIds ?? new List<BlobId>();
        }

        internal void AddTaskExceptionInfoId([NotNull] BlobId taskExceptionInfoId)
        {
            if(TaskExceptionInfoIds != null)
                TaskExceptionInfoIds.Add(taskExceptionInfoId);
            else
                TaskExceptionInfoIds = new List<BlobId> {taskExceptionInfoId};
        }

        internal void MakeSnapshot()
        {
            snapshot = StaticGrobuf.GetSerializer().Serialize(this);
        }

        [CanBeNull]
        internal TaskMetaInformation TryGetSnapshot()
        {
            if(snapshot == null)
                return null;
            return StaticGrobuf.GetSerializer().Deserialize<TaskMetaInformation>(snapshot);
        }

        public override string ToString()
        {
            return string.Format("[Name: {0}, Id: {1}, Attempts: {2}, ParentTaskId: {3}, TaskGroupLock: {4}, State: {5}, TraceId: {6}, TaskDataId: {7}, TaskExceptionInfoIds: {8}]",
                                 Name, Id, Attempts, ParentTaskId, TaskGroupLock, State, TraceId, TaskDataId, TaskExceptionInfoIds == null ? "NONE" : string.Join("; ", TaskExceptionInfoIds.Select(x => x.ToString())));
        }

        private byte[] snapshot;
    }
}