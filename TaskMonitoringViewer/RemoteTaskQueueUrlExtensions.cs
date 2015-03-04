using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer
{
    public static class RemoteTaskQueueUrlExtensions
    {
         public static string GetTaskDetailsUrl(this UrlHelper urlHelper, string taskId, int pageNumber = 0, string searchRequestId = null)
         {
             return urlHelper.Action("Show", "RemoteTaskQueue", new {id = taskId, pageNumber, searchRequestId});
         }

         public static string GetCancelTaskUrl(this UrlHelper urlHelper, string taskId)
         {
             return urlHelper.Action("Cancel", "RemoteTaskQueue", new {id = taskId});
         }

         public static string GetRerunTaskUrl(this UrlHelper urlHelper, string taskId)
         {
             return urlHelper.Action("Rerun", "RemoteTaskQueue", new {id = taskId});
         }

         public static string GetTaskDataBytesUrl(this UrlHelper urlHelper, string taskId, string path)
         {
             return urlHelper.Action("GetBytes", "RemoteTaskQueue", new {id = taskId, path});
         }

         public static string GetSearchUrl(this UrlHelper urlHelper)
         {
             return urlHelper.Action("Search", "RemoteTaskQueue");
         }
    }
}