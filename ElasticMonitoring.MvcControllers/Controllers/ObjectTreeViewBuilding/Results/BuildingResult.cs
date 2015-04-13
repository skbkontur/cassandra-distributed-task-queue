using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results
{
    internal class BuildingResult : IBuildingResult
    {
        public BuildingResult(ObjectTreeModel result)
        {
            Result = result.Value;
        }

        public BuildingResult(ObjectTreeModelValue value)
        {
            Result = value;
        }

        public ObjectTreeModelValue Result { get; private set; }
    }
}