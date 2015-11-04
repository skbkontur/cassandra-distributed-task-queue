using JetBrains.Annotations;

namespace RemoteQueue.Configuration
{
    public interface ITaskTopicResolver
    {
        [NotNull]
        string[] GetAllTaskTopics();

        [NotNull]
        string GetTaskTopic([NotNull] string taskName);
    }
}