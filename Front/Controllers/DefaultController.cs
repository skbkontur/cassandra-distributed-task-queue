using System.Web.Mvc;

using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Models.Default;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Controllers
{
    public class DefaultController : ControllerBase
    {
        public DefaultController(ControllerBaseParameters parameters)
            : base(parameters)
        {
        }

        public ActionResult Debug(string backUrl)
        {
            return View("DebugView", new DefaultModel
                {
                    BackUrl = backUrl
                });
        }
    }
}