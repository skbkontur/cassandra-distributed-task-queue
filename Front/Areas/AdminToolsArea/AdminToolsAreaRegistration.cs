using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.AdminToolsArea
{
    public class AdminToolsAreaRegistration : AreaRegistration
    {
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("AdminTools", @"AdminTools/{controller}/{action}", new { action = "Run", controller = "Task" });
        }

        public override string AreaName { get { return "AdminTools"; } }
    }
}