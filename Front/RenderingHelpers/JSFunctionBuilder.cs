using System.Reflection;

using SKBKontur.Catalogue.Core.Web.Models.ModelConfigurations;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers
{
    public class JSFunctionBuilder : JSFunctionBuilderBase
    {
        public JSFunctionBuilder(IGuidFactory guidFactory)
            : base(guidFactory)
        {
        }

        protected override JSFunction GetJSFunction(MethodInfo method)
        {
            return null;
        }
    }
}