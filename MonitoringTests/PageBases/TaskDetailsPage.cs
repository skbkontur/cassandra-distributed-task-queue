using OpenQA.Selenium;

using SKBKontur.Catalogue.WebTestCore.SystemControls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases
{
    public class TaskDetailsPage : CommonPageBase
    {
        public TaskDetailsPage()
        {
            TaskId = new StaticText("TaskId");
            TaskState = new StaticText(By.ClassName("taskState"));
            TaskName = new StaticText("TaskName");
            EnqueueTicks = new StaticText("EnqueueTicks");
            StartExecutedTicks = new StaticText("StartExecutedTicks");
            Attempts = new StaticText("Attempts");
            ParentTaskId = new Link("ParentTaskId");
            TaskData = new StaticText("TaskData");
            TaskList = new Link(By.LinkText("Вернуться в Task list"));
        }

        public override void BrowseWaitVisible()
        {
            TaskState.WaitVisible();
            TaskId.WaitVisible();
        }

        public TasksListPage GoToTasksListPage()
        {
            TaskList.Click();
            return GoTo<TasksListPage>();
        }

        public StaticText TaskId { get; private set; }
        public StaticText TaskState { get; private set; }
        public StaticText TaskName { get; private set; }
        public StaticText EnqueueTicks { get; private set; }
        public StaticText StartExecutedTicks { get; private set; }
        public StaticText Attempts { get; private set; }
        public Link ParentTaskId { get; private set; }
        public StaticText TaskData { get; private set; }
        public Link TaskList { get; private set; }
    }
}