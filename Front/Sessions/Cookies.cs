using System.Web;

using SKBKontur.Catalogue.Core.Web.CookiesManagement;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Sessions
{
    public class Cookies : CookiesBase
    {
        public Cookies(HttpContextBase httpContext)
            : base(httpContext, cookiesPrefix)
        {
        }

        private const string cookiesPrefix = "remoteTaskQueue";
    }
}