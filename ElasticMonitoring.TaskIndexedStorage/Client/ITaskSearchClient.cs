namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client
{
    public interface ITaskSearchClient
    {
        TaskSearchResponse Search(TaskSearchRequest taskSearchRequest, int from, int size);
    }
}