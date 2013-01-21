using System;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.Controls;
using SKBKontur.Catalogue.WebTestCore.SystemControls;
using SKBKontur.Catalogue.WebTestCore.TestSystem;

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

            ShowPanel = new Link(new ByNthOfClass("slide_title_link_link", 0));

            MinimalStartTicksTimeFrom = new TextInput("SearchPanel_MinimalStartTicks_From_Time");
            MinimalStartTicksDateFrom = new TextInput("SearchPanel_MinimalStartTicks_From_Date");
            MinimalStartTicksTimeTo = new TextInput("SearchPanel_MinimalStartTicks_To_Time");
            MinimalStartTicksDateTo = new TextInput("SearchPanel_MinimalStartTicks_To_Date");

            TaskNames = new[]
                {
                    new CheckBox("SearchPanel_TaskNames_0_Value"),
                    new CheckBox("SearchPanel_TaskNames_1_Value"),
                    new CheckBox("SearchPanel_TaskNames_2_Value"),
                };

            TaskId = new TextInput("SearchPanel_TaskId");

            Search = new ButtonInput("Search");

            New = new CheckBox("SearchPanel_TaskStates_0_Value");
            Finished = new CheckBox("SearchPanel_TaskStates_4_Value");
            InProcess = new CheckBox("SearchPanel_TaskStates_1_Value");
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
            var moscowDateTime = dateTime.HasValue ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime.Value, TimeZoneInfo.Utc.Id, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time").Id)
                                     : new DateTime();
            date.SetValue(dateTime.HasValue ? string.Format("{0:D2}.{1:D2}.{2:D4}", moscowDateTime.Day, moscowDateTime.Month, moscowDateTime.Year) : "");
            time.SetValue(dateTime.HasValue ? string.Format("{0:D2}:{1:D2}:{2:D2}", moscowDateTime.Hour, moscowDateTime.Minute, moscowDateTime.Second) : "");
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

        public Link ShowPanel { get; private set; }

        public CheckBox[] TaskNames { get; private set; }

        public TextInput TaskId { get; private set; }

        public CheckBox New { get; private set; }
        public CheckBox Finished { get; private set; }
        public CheckBox InProcess { get; private set; }

        public ButtonInput Search { get; private set; }
    }
}