using System.Web.Mvc;
using System.Web.Mvc.Html;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Paging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.RenderingHelpers
{
    public static class HtmlPaginatorPrimitives
    {
        public static MvcHtmlString Paginator(this HtmlHelper htmlHelper, IPaginatorModelData model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/Paging/_Paginator.cshtml", model);
        }

        public static MvcHtmlString DotsLinkPage(this HtmlHelper htmlHelper, PaginatorPageLinkModelData model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/Paging/_DotsLinkPage.cshtml", model);
        }

        public static MvcHtmlString LinkPage(this HtmlHelper htmlHelper, PaginatorPageLinkModelData model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/Paging/_LinkPage.cshtml", model);
        }

        public static MvcHtmlString CurrentPage(this HtmlHelper htmlHelper, PaginatorPageLinkModelData model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/Paging/_CurrentPage.cshtml", model);
        }

        public static MvcHtmlString PrevLinkPage(this HtmlHelper htmlHelper, PaginatorPageLinkModelData model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/Paging/_PrevLinkPage.cshtml", model);
        }

        public static MvcHtmlString NextLinkPage(this HtmlHelper htmlHelper, PaginatorPageLinkModelData model)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/Paging/_NextLinkPage.cshtml", model);
        }

        public static MvcHtmlString PrevPage(this HtmlHelper htmlHelper)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/Paging/_PrevPage.cshtml");
        }

        public static MvcHtmlString NextPage(this HtmlHelper htmlHelper)
        {
            return htmlHelper.Partial("~/TaskMonitoringViewer/Paging/_NextPage.cshtml");
        }
    }
}