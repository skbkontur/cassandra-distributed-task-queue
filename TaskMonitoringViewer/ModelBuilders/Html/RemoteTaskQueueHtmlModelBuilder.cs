using SKBKontur.Catalogue.Core.Web.Blocks.Button;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Html;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.Html
{
    internal class RemoteTaskQueueHtmlModelBuilder : IRemoteTaskQueueHtmlModelBuilder
    {
        public RemoteTaskQueueHtmlModelBuilder(IHtmlModelsCreator<RemoteTaskQueueModelData> htmlModelsCreator)
        {
            this.htmlModelsCreator = htmlModelsCreator;
        }

        public SearchPanelHtmlModel Build(RemoteTaskQueueModel pageModel)
        {
            return new SearchPanelHtmlModel
                {
                    TaskName = htmlModelsCreator.TextBoxFor(pageModel, x => x.TaskName, new TextBoxOptions
                        {
                            Size = TextBoxSize.Large
                        }),
                    SearchButton = htmlModelsCreator.ButtonFor(new ButtonOptions
                    {
                        Action = "Search",
                        Id = "Search",
                        Title = "Search",
                        ValidationType = ActionValidationType.All
                    })
                };
        }

        private readonly IHtmlModelsCreator<RemoteTaskQueueModelData> htmlModelsCreator;
    }
}