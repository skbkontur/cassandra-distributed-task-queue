using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTaskCollection
    {
        void AddTask(Task task);
        Task GetTask(string taskId);
        Task[] GetTasks(string[] taskIds);
    }
}