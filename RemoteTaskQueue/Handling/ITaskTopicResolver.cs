using JetBrains.Annotations;

namespace RemoteQueue.Handling
{
    public interface ITaskTopicResolver
    {
        [NotNull]
        string[] GetAllTaskTopics();

        [NotNull]
        string GetTaskTopic([NotNull] string taskName);
    }
}