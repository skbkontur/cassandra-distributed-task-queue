using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results
{
    public class NoResult : IBuildingResult
    {
        public ObjectTreeModelValue Result { get { return null; } }
        public static IBuildingResult Instance { get { return instance; } }
        private static readonly IBuildingResult instance = new NoResult();

    }
}