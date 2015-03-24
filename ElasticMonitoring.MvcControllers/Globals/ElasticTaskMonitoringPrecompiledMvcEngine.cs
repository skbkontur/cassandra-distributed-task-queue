using System.Collections.Generic;

using SKBKontur.Catalogue.ViewPrecompilation.SKBKontur.Catalogue.Core.DatabaseWebViewer.Globals;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Globals
{
    public class ElasticTaskMonitoringPrecompiledMvcEngine : CataloguePrecompiledMvcEngine
    {
        public ElasticTaskMonitoringPrecompiledMvcEngine()
            : base(typeof(ElasticTaskMonitoringPrecompiledMvcEngine).Assembly, "~/ElasticMonitoring/")
        {
            UsePhysicalViewsIfNewer = true;
        }

        protected override IEnumerable<string> GetAlternativePaths(string virtualPath)
        {
            if(virtualPath.StartsWith(VirtualPathPrefix) && !virtualPath.StartsWith(VirtualPathPrefix + "Views/"))
                yield return virtualPath.Replace(VirtualPathPrefix, VirtualPathPrefix + "Views/");
            foreach(var alternativePath in base.GetAlternativePaths(virtualPath))
                yield return alternativePath;
        }
    }
}