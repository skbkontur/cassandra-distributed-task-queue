using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public interface IHandleTaskCollection
    {
        ColumnInfo AddTask(Task task);
        Task GetTask(string taskId);
        Task[] GetTasks(string[] taskIds);
    }
}