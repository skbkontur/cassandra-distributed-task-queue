using RemoteQueue.Handling;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public interface ITaskQueue
    {
        bool Stopped { get; }
        bool QueueTask(HandlerTask handlerTask);
        long GetQueueLength();
        void Start();
        void StopAndWait(int timeout = 10000);
    }
}