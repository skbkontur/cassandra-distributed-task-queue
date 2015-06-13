using System.Web.Mvc;
using System.Web.Routing;

using GroboTrace;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.Core.ObjectTreeWebViewer.Globals;
using SKBKontur.Catalogue.Core.Web.Globals;
using SKBKontur.Catalogue.Core.Web.Globals.ViewPrecompilation;
using SKBKontur.Catalogue.Core.Web.RenderingHelpers;
using SKBKontur.Catalogue.Core.Web.RenderingHelpers.Webpack;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Globals;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Controllers;
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
            engineCollection.RegisterEngine(new ObjectTreeViewerPrecompiledMvcEngine());
            engineCollection.RegisterEngine(new ElasticTaskMonitoringPrecompiledMvcEngine());
        }

        protected override void OnBeginRequest()
        {
            BeginRequestHandler.BeginRequest(Context);
        }

        private static void OverrideRenderAdminToolsStylesAndScriptsHandler(HtmlHelper html)
        {
            html.ViewContext.Writer.WriteLine(html.RenderWebpackEntryScript("webworms-initializer").ToString());
            html.ViewContext.Writer.WriteLine(html.RenderWebpackEntryStyle("webworms-bundle").ToString());
            html.ViewContext.Writer.WriteLine(html.RenderWebpackEntryStyle("webworms-admintools-bundle").ToString());
            html.ViewContext.Writer.WriteLine(html.RenderWebpackEntryScript("webworms-bundle").ToString());
        }

        protected override void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Default", "{controller}/{action}", new {action = "Run"}, new[] {GetControllersNamespace()});
        }

        protected override void DoConfigure()
        {
            HtmlHelpers.OverrideRenderAdminToolsStylesAndScripts = OverrideRenderAdminToolsStylesAndScriptsHandler;
            Configurator.Configure(Container);
            Container.ConfigureWebpackPathProvider();
        }

        protected override void ConfigureTracingWrapper(TracingWrapperConfigurator configurator)
        {
        }

        protected override string ConfigFileName { get { return "front.csf"; } }

        private static string GetControllersNamespace()
        {
            return typeof(DefaultController).Namespace;
        }
    }
}