using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Core.Web.ReferencesHelpers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers
{
    public class EmptyHtmlModelsCreator<TData> : HtmlModelsCreatorBase<TData>, IEmptyHtmlModelsCreator<TData> where TData : ModelData
    {
        public EmptyHtmlModelsCreator(
            ISelectModelBuilder selectModelBuilder,
            IHtmlModelTemplateBuilder htmlModelTemplateBuilder)
            : base(selectModelBuilder, htmlModelTemplateBuilder)
        {
        }
    }
}