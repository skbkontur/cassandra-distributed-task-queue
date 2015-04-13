using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.AdminToolsArea.Controllers
{
    public class TasksController : TasksBaseController
    {
        public TasksController(TasksBaseControllerParameters baseParameters)
            : base(baseParameters)
        {
        }

        protected override string GetAdminToolsActions()
        {
            return Url.Action("Debug", "Default");
        }

        protected override bool CurrentUserHasAccessToReadAction()
        {
            return true;
        }

        protected override bool CurrentUserHasAccessToTaskData()
        {
            return true;
        }

        protected override bool CurrentUserHasAccessToWriteAction()
        {
            return true;
        }
    }
}