using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex
{
    public interface IChildTaskIndex
    {
        void AddMeta(TaskMetaInformation meta);
        string[] GetChildTaskIds(string taskId);
    }
}