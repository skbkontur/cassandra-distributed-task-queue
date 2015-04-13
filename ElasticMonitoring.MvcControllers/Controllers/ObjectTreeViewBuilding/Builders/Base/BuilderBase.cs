using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base
{
    internal abstract class BuilderBase : IBuilder
    {
        protected IBuildingResult RawHtml(string html)
        {
            return new BuildingResult(new ObjectTreeModelValue
                {
                    IsHtml = true,
                    Value = html
                });
        }

        protected IBuildingResult StringValue(string value)
        {
            return new BuildingResult(new ObjectTreeModelValue
                {
                    IsHtml = false,
                    Value = value
                });
        }

        public abstract IBuildingResult Build(object targetObject, BuildingContext buildingContext);
    }
}