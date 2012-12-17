using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.Controls;
using SKBKontur.Catalogue.WebTestCore.SystemControls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases
{
    public class TasksListPage : CommonPageBase
    {
        public override void BrowseWaitVisible()
        {
            NextPage = new Link("Paginator_Next");
            PrevPage = new Link("Paginator_Prev");
        }

        public void CheckTaskListItemsCount(int expectedCount)
        {
            GetTaskListItem(expectedCount - 1).WaitPresence();
            GetTaskListItem(expectedCount).WaitAbsence();
        }

        public TaskDetailsPage GoToTaskDetails(int index)
        {
            GetTaskListItem(index).TaskId.Click();
            return GoTo<TaskDetailsPage>();
        }

        public TaskDetailsPage GoToParentTaskDetails(int index)
        {
            GetTaskListItem(index).ParentTaskId.Click();
            return GoTo<TaskDetailsPage>();
        }

        public TaskListItem GetTaskListItem(int index)
        {
            return new TaskListItem(index);
        }

        public TasksListPage GoToNextPage()
        {
            NextPage.Click();
            return GoTo<TasksListPage>();
        }

        public TasksListPage GoToPrevPage()
        {
            PrevPage.Click();
            return GoTo<TasksListPage>();
        }

        public Link NextPage { get; private set; }
        public Link PrevPage { get; private set; }
    }
}