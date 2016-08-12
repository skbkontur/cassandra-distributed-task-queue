using SKBKontur.Catalogue.Objects.TimeBasedUuid;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.TaskDetailsTreeView
{
    internal class TimeGuidBuilder : BuilderBase
    {
        public override IBuildingResult Build(object targetObject, BuildingContext buildingContext)
        {
            if(buildingContext.MemberBuildingContext.DeclaredType == typeof(TimeGuid))
            {
                var guid = ((TimeGuid)targetObject).ToGuid();
                // TODO Чтобы сделать search link надо сначала запилить сериализацию TimeGuid-ов в Elastic
                return StringValue(guid.ToString());
            }
            return NoResult.Instance;
        }
    }
}