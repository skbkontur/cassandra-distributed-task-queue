namespace RemoteQueue.Handling
{
    public interface ITaskHandlerCollection
    {
        ITaskHandler CreateHandler(string taskName);
        bool ContainsHandlerFor(string taskName);
    }
}