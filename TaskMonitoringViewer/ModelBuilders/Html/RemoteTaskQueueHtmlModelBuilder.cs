using SKBKontur.Catalogue.Core.Web.Blocks.Button;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Html;

using System.Linq;

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
                    TaskName = htmlModelsCreator.SelectBoxFor(pageModel, x => x.SearchPanel.TaskName, new SelectBoxOptions
                        {
                            Size = SelectBoxSize.Medium,
                            ReferenceConfig = new ReferenceConfig
                                {
                                    ReferenceType = "TaskNames",
                                    NeedEmptyValue = true,
                                    SelectBoxElements = pageModel.Data.SearchPanel.AllowedTaskNames.Select(x => new SelectBoxElement
                                        {
                                            Text = x,
                                            Value = x
                                        }).ToArray()
                                }
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