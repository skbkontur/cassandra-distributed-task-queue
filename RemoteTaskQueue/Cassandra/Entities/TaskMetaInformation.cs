namespace RemoteQueue.Cassandra.Entities
{
    public class TaskMetaInformation
    {
        public override string ToString()
        {
            return string.Format("[Name: {0}, Id: {1}, Attempts: {2}, ParentTaskId: {3}, TaskGroupLock: {4}, State: {5}]", 
                Name, Id, Attempts, ParentTaskId, TaskGroupLock, State);
        }

        public string Name { get; set; }
        public string Id { get; set; }

        public long Ticks { get; set; }
        public long MinimalStartTicks { get; set; }
        public long? StartExecutingTicks { get; set; }
        public long? FinishExecutingTicks { get; set; }

        public long? LastModificationTicks { get; set; }

        public TaskState State { get; set; }
        public int Attempts { get; set; }

        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }

        internal void MakeSnapshot()
        {
            snapshot = StaticGrobuf.GetSerializer().Serialize(this);
        }

        internal TaskMetaInformation GetSnapshot()
        {
            if(snapshot == null) return null;
            return StaticGrobuf.GetSerializer().Deserialize<TaskMetaInformation>(snapshot);
        }

        private byte[] snapshot;
    }
}