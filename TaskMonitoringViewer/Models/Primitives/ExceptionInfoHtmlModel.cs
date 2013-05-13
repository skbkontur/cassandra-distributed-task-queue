using System.Web.Mvc;
using System.Web.Mvc.Html;

using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives
{
    public class ExceptionInfoHtmlModel : IRenderHtmlModel
    {
        public string Id { get; set; }
        public MvcHtmlString ToHtmlString(HtmlHelper htmlHelper)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/_ExceptionInfo.cshtml", this);
        }

        public string ExceptionMessageInfo { get; set; }
    }
}