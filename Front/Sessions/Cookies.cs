using System.Web;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.Core.Web.CookiesManagement;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Sessions
{
    public class Cookies : CookiesBase
    {
        public Cookies(HttpContextBase httpContext, IApplicationSettings applicationSettings)
            : base(httpContext, applicationSettings, cookiesPrefix)
        {
        }

        private const string cookiesPrefix = "remoteTaskQueue";
    }
}