using System;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders
{
    internal class DateTimesBuilder : BuilderBase
    {
        public override IBuildingResult Build(object targetObject, BuildingContext buildingContext)
        {
            var propertyType = buildingContext.MemberBuildingContext.DeclaredType;
            var value1 = targetObject;
            if(propertyType == typeof(DateTime))
            {
                var value = (DateTime)value1;
                return StringValue(value.ToString("dd.MM.yyyy HH:mm:ss") + " (UTC) " + value.Ticks);
            }
            if(propertyType == typeof(DateTime?))
            {
                var value = (DateTime?)value1;
                if(value.HasValue)
                    return StringValue(value.Value.ToString("dd.MM.yyyy HH:mm:ss") + " (UTC) " + value.Value.Ticks);
                return RawHtml("<i>null</i>");
            }
            return NoResult.Instance;
        }
    }
}