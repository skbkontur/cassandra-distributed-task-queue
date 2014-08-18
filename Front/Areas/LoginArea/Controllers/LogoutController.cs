using System.Web.Mvc;

using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.CookiesManagement;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Controllers
{
    public class LogoutController : ControllerBase
    {
        public LogoutController(ControllerBaseParameters baseParameters)
            : base(baseParameters)
        {
        }

        public ActionResult Run()
        {
            Cookies.SessionString = null;
            Cookies.KonturPortalTokenString = new Cookie<string>(null, null, ApplicationSettings.GetKonturPortalDomain());
            return Redirect("/?reason=logout");
        }
    }
}