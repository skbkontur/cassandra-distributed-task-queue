namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client
{
    public class TaskSearchResponse
    {
        public string[] Ids { get; set; }
        public string NextScrollId { get; set; }
        public int TotalCount { get; set; }
    }
}