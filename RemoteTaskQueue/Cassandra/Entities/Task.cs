namespace RemoteQueue.Cassandra.Entities
{
    public class Task
    {
        public byte[] Data { get; set; }
        public TaskMetaInformation Meta { get; set; }
    }
}