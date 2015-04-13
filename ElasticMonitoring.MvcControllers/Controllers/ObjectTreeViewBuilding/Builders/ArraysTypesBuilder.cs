using System.Linq;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders
{
    internal class ArraysTypesBuilder : BuilderBase
    {
        public override IBuildingResult Build(object targetObject, BuildingContext buildingContext)
        {
            if(buildingContext.MemberBuildingContext.DeclaredType.IsArray)
            {
                var array = (object[])targetObject;
                return new SubObjectsResults(array.Select((x, i) => new SubObject
                    {
                        Name = "[" + i + "]", 
                        Value = x,
                        DeclaredType = buildingContext.MemberBuildingContext.DeclaredType.GetElementType()
                    }).ToArray());
            }
            return NoResult.Instance;
        }
    }
}