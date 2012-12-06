namespace RemoteQueue.LocalTasks.TaskQueue
{
    public interface ITaskQueue
    {
        bool Stopped { get; }
        bool QueueTask(ITask task);
        long GetQueueLength();
        void Start();
        void StopAndWait(int timeout = 10000);
    }
}