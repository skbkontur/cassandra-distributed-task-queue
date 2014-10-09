using SKBKontur.Catalogue.WebTestCore.SystemControls;
using SKBKontur.Catalogue.WebTestCore.TestSystem;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.Controls
{
    public class TaskListItem : HtmlControl
    {
        public TaskListItem(int index)
            : base(BY.NthOfClass("adminTools__data__item", index))
        {
            TaskId = new Link(BY.NthOfClass("adminTaskId__link", 0), this);
            TaskState = new StaticText(string.Format("TaskModels_{0}_State", index), this);
            TaskName = new StaticText(string.Format("TaskModels_{0}_Name", index), this);
            EnqueueTime = new DateAndTimeHtmlControl(string.Format("TaskModels_{0}_EnqueueTime", index));
            StartExecutingTime = new DateAndTimeHtmlControl(string.Format("TaskModels_{0}_StartExecutingTime", index));
            FinishExecutingTime = new DateAndTimeHtmlControl(string.Format("TaskModels_{0}_FinishExecutingTime", index));
            MinimalStartTime = new DateAndTimeHtmlControl(string.Format("TaskModels_{0}_MinimalStartTime", index));
            
            Attempts = new StaticText(string.Format("TaskModels_{0}_Attempts", index), this);
            ParentTaskId = new Link(string.Format("TaskModels_{0}_ParentTaskId", index), this);

            cancelLinkId = string.Format("TaskModels_{0}_State_CancelLink", index);
            rerunLinkId = string.Format("TaskModels_{0}_State_RerunLink", index);
            CancelLink = new Link(cancelLinkId, this);
            RerunLink = new Link(rerunLinkId, this);
        }

        public void RerunTask()
        {
            WebDriverCache.WebDriver.ExecuteScript(GetJavaScript(rerunLinkId));
            RerunLink.Click();
            WebDriverCache.WebDriver.ExecuteScript(GetJavaScript(rerunLinkId));
        }

        public void CancelTask()
        {
            WebDriverCache.WebDriver.ExecuteScript(GetJavaScript(cancelLinkId));
            CancelLink.Click();
            WebDriverCache.WebDriver.ExecuteScript(GetJavaScript(cancelLinkId));
        }

        public Link TaskId { get; private set; }
        public StaticText TaskState { get; private set; }
        public StaticText TaskName { get; private set; }
        public DateAndTimeHtmlControl EnqueueTime { get; private set; }
        public DateAndTimeHtmlControl StartExecutingTime { get; private set; }
        public DateAndTimeHtmlControl FinishExecutingTime { get; private set; }
        public DateAndTimeHtmlControl MinimalStartTime { get; private set; }
        public StaticText Attempts { get; private set; }
        public Link ParentTaskId { get; private set; }

        private static string GetJavaScript(string idLocator)
        {
            return string.Format("$('#{0}').toggleClass('noConfirmation')", idLocator);
        }

        private Link CancelLink { get; set; }
        private Link RerunLink { get; set; }
        private readonly string rerunLinkId;
        private readonly string cancelLinkId;
    }
}