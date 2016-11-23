using System.Collections.Generic;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding
{
    public class ObjectTreeModel
    {
        public ObjectTreeModelValue Value { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }

        public IEnumerable<ObjectTreeModel> Children { get { return children; }}

        public void AddChild(ObjectTreeModel child)
        {
            children.Add(child);
        }

        private readonly List<ObjectTreeModel> children = new List<ObjectTreeModel>();
    }
}