using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers
{
    public interface IEmptyHtmlModelsCreator<TData> : IHtmlModelsCreator<TData> where TData : ModelData
    {
    }
}