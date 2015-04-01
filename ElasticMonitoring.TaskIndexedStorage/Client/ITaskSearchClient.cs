namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client
{
    public interface ITaskSearchClient
    {
        TaskSearchResponse SearchNext(string scrollId);
        TaskSearchResponse SearchFirst(TaskSearchRequest taskSearchRequest);
    }
}