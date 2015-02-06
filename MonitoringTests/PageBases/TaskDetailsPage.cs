using SKBKontur.Catalogue.WebTestCore.SystemControls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases
{
    public class TaskDetailsPage : CommonPageBase
    {
        public TaskDetailsPage()
        {
            TaskId = new Link("'TaskMetaInfoModel_TaskId'");
            TaskState = new StaticText("TaskMetaInfoModel_State");
            TaskName = new StaticText("TaskMetaInfoModel_Name");
            EnqueueTime = new StaticText("TaskMetaInfoModel_EnqueueTime");
            StartExecutingTime = new StaticText("TaskMetaInfoModel_StartExecutingTime");
            Attempts = new StaticText("TaskMetaInfoModel_Attempts");
            ParentTaskId = new Link("TaskMetaInfoModel_ParentTaskId");
            TaskData = new StaticText("TaskMetaInfoModel_TaskData");
            TaskList = new Link("Backward_Link");
        }

        public override void BrowseWaitVisible()
        {
            TaskState.WaitVisibleWithRetries();
            TaskId.WaitVisibleWithRetries();
        }

        public TasksListPage GoToTasksListPage()
        {
            TaskList.Click();
            return GoTo<TasksListPage>();
        }

        public Link TaskId { get; private set; }
        public StaticText TaskState { get; private set; }
        public StaticText TaskName { get; private set; }
        public StaticText EnqueueTime { get; private set; }
        public StaticText StartExecutingTime { get; private set; }
        public StaticText Attempts { get; private set; }
        public Link ParentTaskId { get; private set; }
        public StaticText TaskData { get; private set; }
        public Link TaskList { get; private set; }
    }
}