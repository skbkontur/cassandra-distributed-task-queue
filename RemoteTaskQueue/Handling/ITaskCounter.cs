namespace RemoteQueue.Handling
{
    public interface ITaskCounter
    {
        bool CanQueueTask(TaskQueueReason reason);
        bool TryIncrement(TaskQueueReason reason);
        void Decrement(TaskQueueReason reason);
    }
}