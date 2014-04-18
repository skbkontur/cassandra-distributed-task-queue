using System.Web.Mvc;
using System.Web.Routing;

using GroboTrace;

using SKBKontur.Catalogue.Core.Web.Globals;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Controllers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front
{
    public class Global : CatalogueFrontGlobalBase
    {
        protected override void OnError()
        {
            if(!ErrorRedirectHandler.Handle(Context))
                ErrorHtmlHandler.HandleError(Context);
        }

        protected override void OnBeginRequest()
        {
            BeginRequestHandler.BeginRequest(Context);
        }

        protected override void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Default", "{controller}/{action}", new {action = "Run"}, new[] {GetControllersNamespace()});
        }

        protected override void DoConfigure()
        {
            Configurator.Configure(Container);
        }

        protected override void ConfigureTracingWrapper(TracingWrapperConfigurator configurator)
        {
        }

        protected override string ConfigFileName { get { return "frontSettings"; } }

        private static string GetControllersNamespace()
        {
            return typeof(DefaultController).Namespace;
        }
    }
}