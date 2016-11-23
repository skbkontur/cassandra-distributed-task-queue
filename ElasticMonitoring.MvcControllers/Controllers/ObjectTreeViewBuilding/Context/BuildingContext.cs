namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context
{
    class BuildingContext
    {
        public object ObjectBuildingContext { get; set; }
        public MemberBuildingContext MemberBuildingContext {get; set; }
        public object RootObject { get; set; }
    }
}