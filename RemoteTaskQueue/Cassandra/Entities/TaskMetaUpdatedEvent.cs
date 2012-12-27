namespace RemoteQueue.Cassandra.Entities
{
    public class TaskMetaUpdatedEvent
    {
        public long Ticks { get; set; }
        public string TaskId { get; set; }
    }
}