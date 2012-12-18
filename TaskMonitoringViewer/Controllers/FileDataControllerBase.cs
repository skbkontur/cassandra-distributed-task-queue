using System.Web.Mvc;

using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.CassandraStorageCore.FileDataStorage;
using SKBKontur.Catalogue.Core.Web.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    [ResourseGroup(ResourseGroups.AdminResourse)]
    public abstract class FileDataControllerBase : AuthanticatedControllerBase
    {
        protected FileDataControllerBase(RemoteTaskQueueControllerBaseParameters taskControllerBaseParameters, IFileDataStorage fileDataStorage)
            : base(taskControllerBaseParameters.LoggedInControllerBaseParameters)
        {
            this.fileDataStorage = fileDataStorage;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(string fileId)
        {
            var file = fileDataStorage.Read(fileId);
            return File(file.Content, "text/html", file.Filename);
        }

        private readonly IFileDataStorage fileDataStorage;
    }
}