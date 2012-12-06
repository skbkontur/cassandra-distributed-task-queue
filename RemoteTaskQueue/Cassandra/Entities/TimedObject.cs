namespace RemoteQueue.Cassandra.Entities
{
    public class TimedObject
    {
        public string Id { get; set; }
        public long Ticks { get; set; }
    }
}