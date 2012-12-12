using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea
{
    public class LoginAreaRegistration : AreaRegistration
    {
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("Login", @"Login/{controller}/{action}", new { action = "Run" });
        }

        public override string AreaName { get { return "Login"; } }
    }
}