using SKBKontur.Catalogue.Core.Web.Blocks.Button;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Html
{
    public class SearchPanelHtmlModel
    {
        public TextBoxHtmlModel TaskName { get; set; }
        public ButtonHtmlModel SearchButton { get; set; }
    }
}