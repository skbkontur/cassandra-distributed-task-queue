using System.Linq;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.TaskDetailsTreeView
{
    internal class ByteArrayBuilder : BuilderBase
    {
        public override IBuildingResult Build(object targetObject, BuildingContext buildingContext)
        {
            var taskDataBuildingContext = (buildingContext.ObjectBuildingContext as TaskDataBuildingContext);
            if(taskDataBuildingContext == null)
                return NoResult.Instance;
            if(buildingContext.MemberBuildingContext.DeclaredType == typeof(byte[]))
            {
                var urlHelper = taskDataBuildingContext.UrlHelper;
                var value = targetObject;
                return RawHtml(string.Format(@"<a href=""{0}""> <span class=""glyphicon glyphicon-download-alt"" aria-hidden=""true""></span> Скачать ({1} байт)</a>",
                                             urlHelper.Action("GetBytes", new {id = taskDataBuildingContext.TaskId, path = string.Join(".", buildingContext.MemberBuildingContext.PathFromRoot.Select(x => x))}),
                                             ((byte[])value).Length));
            }
            return NoResult.Instance;
        }
    }
}