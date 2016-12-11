namespace RemoteTaskQueue.Monitoring.Storage.Client
{
    public interface ITaskSearchClient
    {
        TaskSearchResponse Search(TaskSearchRequest taskSearchRequest, int from, int size);
    }
}