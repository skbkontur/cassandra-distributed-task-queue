using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders
{
    internal class SubObjectBuildingError
    {
        public string Error { get; set; }
        public Exception Exception { get; set; }
    }
}