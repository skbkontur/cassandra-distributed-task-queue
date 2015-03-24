using System.Web;
using System.Web.Mvc;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers
{
    public static class MagicStaticsExtensions
    {
        public static IHtmlString MagicScript(this HtmlHelper html, string path)
        {
            return html.Raw(string.Format(@"<script type=""text/javascript"" src=""{0}""></script>", new UrlHelper(html.ViewContext.RequestContext).Content(path)));
        }

        public static IHtmlString MagicStyle(this HtmlHelper html, string path)
        {
            return html.Raw(string.Format(@"<link href=""{0}"" rel=""stylesheet"" type=""text/css"">", new UrlHelper(html.ViewContext.RequestContext).Content(path)));
        }
    }
}