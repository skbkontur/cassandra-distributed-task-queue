using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models
{
    public class TaskDetailsModel
    {
        public ObjectTreeModel DetailsTree { get; set; }
        public string TaskName { get; set; }
        public string ExceptionInfo { get; set; }
        public string TaskId { get; set; }
        public TaskState State { get; set; }
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
        public int GlobalIndex { get; private set; }

        public void IncrementGlobalIndex()
        {
            GlobalIndex++;
        }
    }
}