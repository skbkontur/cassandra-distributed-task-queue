using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers
{
    public class EmptyHtmlModelsCreator<TData> : HtmlModelsCreatorBase<TData>, IEmptyHtmlModelsCreator<TData> where TData : ModelData
    {
        public EmptyHtmlModelsCreator(HtmlModelCreatorParameters htmlModelCreatorParameters)
            : base(htmlModelCreatorParameters)
        {
        }
    }
}