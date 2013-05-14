using System.Web.Mvc;
using System.Web.Mvc.Html;

using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives
{
    public class TaskDateTimeHtmlModel : IRenderHtmlModel
    {
        public string Id { get; set; }
        public MvcHtmlString ToHtmlString(HtmlHelper htmlHelper)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/_TaskDateTime.cshtml", this);
        }

        public string DateTime { get; set; }
        public long? Ticks { get; set; }
        public bool HideTicks { get; set; }
    }
}