using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTaskCollection : IHandleTaskCollection
    {
        public HandleTaskCollection(IHandleTasksMetaStorage handleTasksMetaStorage, ITaskDataBlobStorage taskDataStorage)
        {
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.taskDataStorage = taskDataStorage;
        }

        public void AddTask(Task task)
        {
            taskDataStorage.Write(task.Meta.Id, task.Data);
            handleTasksMetaStorage.AddMeta(task.Meta);
        }

        public Task GetTask(string taskId)
        {
            var taskData = taskDataStorage.Read(taskId);
            var meta = handleTasksMetaStorage.GetMeta(taskId);
            return new Task
                {
                    Data = taskData,
                    Meta = meta
                };
        }

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataBlobStorage taskDataStorage;
    }
}