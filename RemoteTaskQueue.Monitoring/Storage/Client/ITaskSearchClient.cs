using JetBrains.Annotations;

namespace RemoteTaskQueue.Monitoring.Storage.Client
{
    public interface ITaskSearchClient
    {
        [NotNull]
        TaskSearchResponse Search([NotNull] TaskSearchRequest taskSearchRequest, int from, int size);
    }
}