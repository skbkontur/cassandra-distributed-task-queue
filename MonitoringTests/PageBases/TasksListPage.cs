using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.Controls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases
{
    public class TasksListPage : CommonPageBase
    {
        public override void BrowseWaitVisible()
        {
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
    }
}