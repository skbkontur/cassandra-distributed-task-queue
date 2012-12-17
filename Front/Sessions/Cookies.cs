﻿using System.Web;

using SKBKontur.Catalogue.Core.Web.CookiesManagement;
using SKBKontur.Catalogue.ServiceLib.Settings;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Sessions
{
    public class Cookies : CookiesBase
    {
        public Cookies(HttpContextBase httpContext, IApplicationSettings applicationSettings)
            : base(httpContext, applicationSettings, cookiesPrefix)
        {
        }

        private const string cookiesPrefix = "rosAlko";
    }
}