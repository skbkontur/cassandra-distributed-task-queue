namespace RemoteQueue.Cassandra.Entities
{
    public class TaskMetaInformation
    {
        public override string ToString()
        {
            return string.Format("Name: {0}, Id: {1}, Attempts: {2}, ParentTaskId: {3}", Name, Id, Attempts, ParentTaskId);
        }

        public string Name { get; set; }
        public string Id { get; set; }
        public long Ticks { get; set; }

        //[Indexed]
        public long MinimalStartTicks { get; set; }
        //[Indexed]
        public long? StartExecutingTicks { get; set; }
        //[Indexed]
        public TaskState State { get; set; }
        //[Indexed]
        public int Attempts { get; set; }
        //[Indexed]
        public string ParentTaskId { get; set; }
    }
}