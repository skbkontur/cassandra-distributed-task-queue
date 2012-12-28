using System;

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

            TicksTimeFrom = new TextInput("SearchPanel_Ticks_From_Time");
            TicksDateFrom = new TextInput("SearchPanel_Ticks_From_Date");
            TicksTimeTo = new TextInput("SearchPanel_Ticks_To_Time");
            TicksDateTo = new TextInput("SearchPanel_Ticks_To_Date");

            MinimalStartTicksTimeFrom = new TextInput("SearchPanel_MinimalStartTicks_From_Time");
            MinimalStartTicksDateFrom = new TextInput("SearchPanel_MinimalStartTicks_From_Date");
            MinimalStartTicksTimeTo = new TextInput("SearchPanel_MinimalStartTicks_To_Time");
            MinimalStartTicksDateTo = new TextInput("SearchPanel_MinimalStartTicks_To_Date");

            TaskName = new Select("SearchPanel_TaskName");
            TaskId = new TextInput("SearchPanel_TaskId");

            Search = new ButtonInput("Search");

            // note не всегда! только в текущих тестах.
            New = new CheckBox("SearchPanel_States_0_Value");
            Finished = new CheckBox("SearchPanel_States_1_Value");
            InProcess = new CheckBox("SearchPanel_States_2_Value");
        }

        public TasksListPage Refresh()
        {
            return RefreshPage(this);
        }

        public void CheckTaskListItemsCount(int expectedCount)
        {
            if(expectedCount > 0)
                GetTaskListItem(expectedCount - 1).WaitPresence();
            GetTaskListItem(expectedCount).WaitAbsence();
        }

        public TaskDetailsPage GoToTaskDetails(int index)
        {
            GetTaskListItem(index).TaskId.Click();
            return GoTo<TaskDetailsPage>();
        }

        public static void SetDateTime(TextInput date, TextInput time, DateTime? dateTime)
        {
            date.SetValue(dateTime.HasValue ? string.Format("{0:D2}.{1:D2}.{2:D4}", dateTime.Value.Day, dateTime.Value.Month, dateTime.Value.Year) : "");
            time.SetValue(dateTime.HasValue ? string.Format("{0:D2}:{1:D2}:{2:D2}", dateTime.Value.Hour, dateTime.Value.Minute, dateTime.Value.Second) : "");
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

        public TasksListPage SearchTasks()
        {
            Search.Click();
            return GoTo<TasksListPage>();
        }

        public TasksListPage GoToPrevPage()
        {
            PrevPage.Click();
            return GoTo<TasksListPage>();
        }

        public Link NextPage { get; private set; }
        public Link PrevPage { get; private set; }

        public TextInput MinimalStartTicksDateFrom { get; private set; }
        public TextInput MinimalStartTicksTimeFrom { get; private set; }
        public TextInput MinimalStartTicksDateTo { get; private set; }
        public TextInput MinimalStartTicksTimeTo { get; private set; }

        public TextInput TicksTimeFrom { get; private set; }
        public TextInput TicksDateFrom { get; private set; }
        public TextInput TicksTimeTo { get; private set; }
        public TextInput TicksDateTo { get; private set; }

        public Select TaskName { get; private set; }
        public TextInput TaskId { get; private set; }

        public CheckBox New { get; private set; }
        public CheckBox Finished { get; private set; }
        public CheckBox InProcess { get; private set; }

        public ButtonInput Search { get; private set; }
    }
}