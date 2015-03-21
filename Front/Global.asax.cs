using System.Web.Mvc;
using System.Web.Routing;

using GroboTrace;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.Core.ObjectTreeWebViewer.Globals;
using SKBKontur.Catalogue.Core.Web.Globals;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Controllers;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Registration;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Globals;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front
{
    public class Global : CatalogueFrontGlobalBase
    {
        protected override void OnError()
        {
            if(!ErrorRedirectHandler.Handle(Context))
                ErrorHtmlHandler.HandleError(Context);
        }

        protected override void ConfigurePrecompiledViewEngines(IPrecompiledViewEngineCollection engineCollection, IApplicationSettings applicationSettings)
        {
            engineCollection.RegisterEngine(new WebWormsCommonPrecompiledMvcEngine());
            engineCollection.RegisterEngine(new WebWormsPrecompiledMvcEngine());
            engineCollection.RegisterEngine(new TaskMonitoringPrecompiledMvcEngine());
            engineCollection.RegisterEngine(new RemoteTaskQueueCounterPrecompiledMvcEngine());
            engineCollection.RegisterEngine(new ObjectTreeViewerPrecompiledMvcEngine());
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