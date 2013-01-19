using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer
{
    public static class RemoteTaskQueueUrlExtensions
    {
         public static string GetTaskDetailsUrl(this UrlHelper urlHelper, string taskId, int pageNumber = 0, string searchRequestId = null)
         {
// ReSharper disable Mvc.AreaNotResolved
             return urlHelper.Action("Show", "RemoteTaskQueue", new {area = "AdminTools", id = taskId, pageNumber, searchRequestId});
// ReSharper restore Mvc.AreaNotResolved
         }

         public static string GetCancelTaskUrl(this UrlHelper urlHelper, string taskId)
         {
// ReSharper disable Mvc.AreaNotResolved
             return urlHelper.Action("Cancel", "RemoteTaskQueue", new {area = "AdminTools", id = taskId});
// ReSharper restore Mvc.AreaNotResolved
         }

         public static string GetRerunTaskUrl(this UrlHelper urlHelper, string taskId)
         {
// ReSharper disable Mvc.AreaNotResolved
             return urlHelper.Action("Rerun", "RemoteTaskQueue", new {area = "AdminTools", id = taskId});
// ReSharper restore Mvc.AreaNotResolved
         }

         public static string GetTaskDataBytesUrl(this UrlHelper urlHelper, string taskId, string path)
         {
// ReSharper disable Mvc.AreaNotResolved
             return urlHelper.Action("GetBytes", "RemoteTaskQueue", new {area = "AdminTools", id = taskId, path});
// ReSharper restore Mvc.AreaNotResolved
         }

         public static string GetSearchUrl(this UrlHelper urlHelper)
         {
// ReSharper disable Mvc.AreaNotResolved
             return urlHelper.Action("Search", "RemoteTaskQueue", new {area = "AdminTools"});
// ReSharper restore Mvc.AreaNotResolved
         }
    }
}