using System;
using System.Collections.Generic;
using System.Reflection;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders
{
    internal class NestedClassTypeBuilder : BuilderBase
    {
        public override IBuildingResult Build(object targetObject, BuildingContext buildingContext)
        {
            if(buildingContext.MemberBuildingContext.DeclaredType.IsClass)
            {
                var subObjects = new List<SubObject>();
                foreach(var propertyInfo in buildingContext.MemberBuildingContext.DeclaredType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    subObjects.Add(TryEvaluateProperty(targetObject, propertyInfo));
                }
                return new SubObjectsResults(subObjects.ToArray());
            }
            return NoResult.Instance;
        }

        private static SubObject TryEvaluateProperty(object targetObject, PropertyInfo propertyInfo)
        {
            object propertyValue;
            try
            {
                propertyValue = propertyInfo.GetValue(targetObject, null);
            }
            catch(Exception e)
            {
                var buildingError = new SubObjectBuildingError
                    {
                        Error = "Couldn't evaluate property",
                        Exception = e
                    };
                return new SubObject
                    {
                        Name = propertyInfo.Name,
                        DeclaredType = buildingError.GetType(),
                        Value = buildingError
                    };
            }
            return new SubObject
                {
                    Name = propertyInfo.Name,
                    DeclaredType = propertyInfo.PropertyType,
                    Value = propertyValue
                };
        }
    }
}