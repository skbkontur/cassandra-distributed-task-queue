using JetBrains.Annotations;

namespace RemoteTaskQueue.Monitoring.Storage.Client
{
    public interface ITaskSearchClient
    {
        TaskSearchResponse Search(TaskSearchRequest taskSearchRequest, int from, int size);
        [NotNull]
        TaskSearchResponse Search([NotNull] TaskSearchRequest taskSearchRequest, int from, int size);
    }
}