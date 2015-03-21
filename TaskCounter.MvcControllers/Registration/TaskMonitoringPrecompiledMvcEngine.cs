using System.Collections.Generic;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Controllers;
using SKBKontur.Catalogue.ViewPrecompilation.SKBKontur.Catalogue.Core.DatabaseWebViewer.Globals;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Registration
{
    public class RemoteTaskQueueCounterPrecompiledMvcEngine : CataloguePrecompiledMvcEngine
    {
        public RemoteTaskQueueCounterPrecompiledMvcEngine()
            : base(typeof(RemoteTaskQueueTaskCounterControllerBase).Assembly, "~/TaskCounter.MvcControllers/")
        {
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