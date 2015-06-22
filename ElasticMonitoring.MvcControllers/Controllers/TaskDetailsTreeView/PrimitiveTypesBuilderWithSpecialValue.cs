using System;
using System.Linq;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.TaskDetailsTreeView
{
    internal class PrimitiveTypesBuilderWithSpecialValue : PrimitiveTypesBuilder
    {
        public override IBuildingResult Build(object targetObject, BuildingContext buildingContext)
        {
            var propertyType = buildingContext.MemberBuildingContext.DeclaredType;
            if(propertyType == typeof(string) || propertyType == typeof(Guid))
            {
                return RawHtml(targetObject +
                               string.Format(@"<a target=""_blank"" href=""{0}"" class=""pull-right"" data-toggle=""tooltip"" data-placement=""left"" title=""Найти все задачи, у которых это поле равно данному значению"" ><span class=""glyphicon glyphicon-search""></span><span>", BuildSearchLink(targetObject, buildingContext)));
            }
            return base.Build(targetObject, buildingContext);
        }

        private string BuildSearchLink(object targetObject, BuildingContext buildingContext)
        {
            var url = (buildingContext.ObjectBuildingContext as TaskDataBuildingContext).UrlHelper;
            return url.Action(
                "Run",
                new
                    {
                        q = "Data." + string.Join(".", buildingContext.MemberBuildingContext.PathFromRoot.Where(x => !x.Contains("["))) + ":" + targetObject,
                        start = DateTime.Now.AddDays(-7).ToString("dd.MM.yyyy"),
                        end = DateTime.Now.ToString("dd.MM.yyyy"),
                    });
        }
    }
}