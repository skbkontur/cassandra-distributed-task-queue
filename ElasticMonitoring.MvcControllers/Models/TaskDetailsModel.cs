using System;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

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
    }

    public class ObjectTreeModel
    {
        private readonly List<ObjectTreeModel> children = new List<ObjectTreeModel>();
        public string Name { get; set; }
        public string Value { get; set; }

        public IEnumerable<ObjectTreeModel> Children { get { return children; }}

        public void AddChild(ObjectTreeModel child)
        {
            children.Add(child);
        }
    }

    public class ObjectTreeModelRenderContext
    {
        public ObjectTreeModelRenderContext(int index = 0)
        {
            GlobalIndex = index;
        }

        public int GlobalIndex { get; private set; }

        public void IncrementGlobalIndex()
        {
            GlobalIndex++;
        }
    }
}