using System;
using System.Linq;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders
{
    internal class PrimitiveTypesBuilder : BuilderBase
    {
        public override IBuildingResult Build(object targetObject, BuildingContext buildingContext)
        {
            if(IsPrimitiveType(buildingContext.MemberBuildingContext.DeclaredType))
                return StringValue(targetObject.ToString());
            return NoResult.Instance;
        }

        private bool IsPrimitiveType(Type propertyType)
        {
            return new[]
                {
                    typeof(string),
                    typeof(int),
                    typeof(double),
                    typeof(long),
                }.Contains(propertyType);
        }
    }
}