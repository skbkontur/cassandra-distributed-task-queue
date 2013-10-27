using System.Linq;

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
            return GetTasks(new[] {taskId}).First();
        }

        public Task[] GetTasks(string[] taskIds)
        {
            var taskDatas = taskDataStorage.ReadQuiet(taskIds);
            var metas = handleTasksMetaStorage.GetMetasQuiet(taskIds);
            return taskDatas.Zip(
                metas,
                (t, m) =>
                new Task
                    {
                        Data = t,
                        Meta = m
                    }
                )
                .Where(x => x.Meta != null && x.Data != null)
                .ToArray();
        }

        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskDataBlobStorage taskDataStorage;
    }
}