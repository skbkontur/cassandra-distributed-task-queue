using JetBrains.Annotations;

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

        [NotNull]
        public string TaskDataId { get { return taskDataId ?? Id; } set { taskDataId = value; } }

        [NotNull]
        public string TaskExceptionId { get { return taskExceptionId ?? Id; } set { taskExceptionId = value; } }

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
            return string.Format("[Name: {0}, Id: {1}, Attempts: {2}, ParentTaskId: {3}, TaskGroupLock: {4}, State: {5}, TraceId: {6}, TaskDataId: {7}, TaskExceptionId: {8}]",
                                 Name, Id, Attempts, ParentTaskId, TaskGroupLock, State, TraceId, TaskDataId, TaskExceptionId);
        }

        private byte[] snapshot;
        private string taskDataId;
        private string taskExceptionId;
    }
}