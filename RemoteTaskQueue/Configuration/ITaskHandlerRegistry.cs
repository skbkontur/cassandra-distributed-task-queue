using JetBrains.Annotations;

using RemoteQueue.Handling;

namespace RemoteQueue.Configuration
{
    public interface ITaskHandlerRegistry
    {
        [NotNull]
        string[] GetAllTaskTopicsToHandle();

        bool ContainsHandlerFor([NotNull] string taskName);

        [NotNull]
        ITaskHandler CreateHandlerFor([NotNull] string taskName);
    }
}