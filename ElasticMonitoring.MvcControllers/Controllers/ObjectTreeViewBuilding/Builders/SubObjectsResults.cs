using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders
{
    internal class SubObjectsResults : IBuildingResult
    {
        public SubObjectsResults(SubObject[] subObjects)
        {
            SubObjects = subObjects;
        }

        public SubObject[] SubObjects { get; set; }
        public ObjectTreeModelValue Result { get { return null; } }
    }
}