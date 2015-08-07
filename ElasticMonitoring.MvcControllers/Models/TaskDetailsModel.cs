using System;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models
{
    public class TaskDetailsModel
    {
        public ObjectTreeModel DetailsTree { get; set; }
        public TaskState State { get; set; }

        public string TaskName { get; set; }
        public string ExceptionInfo { get; set; }
        public string TaskId { get; set; }
        public DateTime? EnqueueTime { get; set; }
        public DateTime? StartExecutedTime { get; set; }
        public DateTime? FinishExecutedTime { get; set; }
        public DateTime? MinimalStartTime { get; set; }
        public int AttemptCount { get; set; }
        public string ParentTaskId { get; set; }
        public string[] ChildTaskIds { get; set; }
        public int TaskGroupLock { get; set; }
        public bool AllowControlTaskExecution { get; set; }
        public string HelloImage { get; set; }
    }
}