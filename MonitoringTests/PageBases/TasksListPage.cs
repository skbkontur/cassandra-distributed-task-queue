using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.Controls;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests;
using SKBKontur.Catalogue.WebTestCore.SystemControls;
using SKBKontur.Catalogue.WebTestCore.TestSystem;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases
{
    public class TasksListPage : CommonPageBase
    {
        public TasksListPage()
        {
            NextPage = new Link("Paginator_Next");
            PrevPage = new Link("Paginator_Prev");

            TicksTimeFrom = new TextInput("SearchPanel_Ticks_From_Time");
            TicksDateFrom = new TextInput("SearchPanel_Ticks_From_Date");
            TicksTimeTo = new TextInput("SearchPanel_Ticks_To_Time");
            TicksDateTo = new TextInput("SearchPanel_Ticks_To_Date");

            ShowPanel = new Link(BY.NthOfClass("slide_title_link_link", 0));

            TasksCount = new StaticText("TaskCount");

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

        public override void BrowseWaitVisible()
        {
        }

        public TasksListPage Refresh()
        {
            return RefreshPage(this);
        }

        public TasksListPage RefreshUntilState(int index, string state)
        {
            return RefreshUntil(this, page => page.GetTaskListItem(index).TaskState.GetText() == state);
        }

        public TasksListPage SearchUntilTaskListItemsCountIs(int expectedCount, int timeout = 20000)
        {
            var start = DateTime.UtcNow;
            var page = this;
            TasksCount.WaitPresenceWithRetries();
            while(DateTime.UtcNow.Subtract(start) < TimeSpan.FromMilliseconds(timeout))
            {
                page = page.SearchTasks();
                if(expectedCount.ToString(CultureInfo.InvariantCulture) == TasksCount.GetText())
                    return page;
            }
            Assert.Fail("Недождались ожидаесого кол-во задач в списке за {0}. Ожидалось: \"{1}\", но было \"{2}.\"", timeout, expectedCount, TasksCount.GetText());
            return null;
        }

        public IEnumerable<TaskInfo> GetTaskInfos(int count)
        {
            var taskInfos = new List<TaskInfo>();
            for(var i = 0; i < count; i++)
            {
                var taskListItem = GetTaskListItem(i);
                taskInfos.Add(new TaskInfo
                    {
                        TaskId = taskListItem.TaskId.GetText(),
                        EnqueueTime = taskListItem.EnqueueTime.GetDateTimeUtc(),
                        MinimalStartTime = taskListItem.MinimalStartTime.GetDateTimeUtc(),
                    });
            }
            return taskInfos;
        }

        public TasksListPage RefreshUntilTaskRowIsPresent(int expectedCount)
        {
            return RefreshUntil(this, page =>
                {
                    page.GetTaskListItem(expectedCount - 1).WaitPresenceWithRetries();
                    return true;
                });
        }

        public TasksListPage RefreshUntilAllTasksInState(int expectedTasksCount, string expectedState)
        {
            return RefreshUntil(this, page => Enumerable.Range(0, expectedTasksCount).All(index =>
                {
                    var taskListItem = page.GetTaskListItem(index);
                    return taskListItem.IsPresent && taskListItem.TaskState.GetText() == expectedState;
                }));
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
            date.WaitEnabledWithRetries();
            time.WaitEnabledWithRetries();
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

        public TasksListPage RerunTask(int index)
        {
            GetTaskListItem(index).RerunTask();
            return GoTo<TasksListPage>();
        }

        public TasksListPage CancelTask(int index)
        {
            GetTaskListItem(index).CancelTask();
            return GoTo<TasksListPage>();
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
        public readonly StaticText TasksCount;
    }
}