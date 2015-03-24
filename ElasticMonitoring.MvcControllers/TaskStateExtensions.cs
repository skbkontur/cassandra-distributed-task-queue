using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers
{
    public static class TaskStateExtensions
    {
        public static bool CanRerunTask(this TaskState taskState)
        {
            return taskState == TaskState.Fatal || taskState == TaskState.Finished || taskState == TaskState.WaitingForRerun || taskState == TaskState.WaitingForRerunAfterError;
        }

        public static bool CanCancelTask(this TaskState taskState)
        {
            return taskState == TaskState.New || taskState == TaskState.WaitingForRerun || taskState == TaskState.WaitingForRerunAfterError;
        }
    }
}