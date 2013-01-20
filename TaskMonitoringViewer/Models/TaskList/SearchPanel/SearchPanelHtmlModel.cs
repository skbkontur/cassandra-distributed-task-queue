using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList.SearchPanel
{
    public class SearchPanelHtmlModel
    {
        public CheckboxWithValue[] TaskStates { get; set; }
        public CheckboxWithValue[] TaskNames { get; set; }
        public TextBoxHtmlModel TaskId { get; set; }
        public TextBoxHtmlModel ParentTaskId { get; set; }
        public ActionButtonHtmlModel SearchButton { get; set; }
        public DateTimeRangeHtmlModel Ticks { get; set; }
        public DateTimeRangeHtmlModel StartExecutedTicks { get; set; }
        public DateTimeRangeHtmlModel FinishExecutedTicks { get; set; }
        public DateTimeRangeHtmlModel MinimalStartTicks { get; set; }
    }

    public class CheckboxWithValue
    {
        public TextBoxHtmlModel Value { get; set; }
        public CheckBoxHtmlModel CheckBox { get; set; }
    }
}