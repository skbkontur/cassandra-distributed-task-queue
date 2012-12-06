namespace RemoteQueue.Handling
{
    public interface ITaskCounter
    {
        bool CanQueueTask();
        bool TryIncrement();
        void Decrement();
    }
}