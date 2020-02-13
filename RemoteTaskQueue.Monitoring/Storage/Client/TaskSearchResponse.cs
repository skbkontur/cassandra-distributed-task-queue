namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Client
{
    public class TaskSearchResponse
    {
        public string[] Ids { get; set; }
        public int TotalCount { get; set; }
    }
}