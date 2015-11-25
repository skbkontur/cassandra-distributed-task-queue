using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class DateTimeRangeFilterTest : FiltersTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            addTasksInfo = AddTasks(7,
                                    new Creater("AlphaTaskData", 3600, () => new AlphaTaskData()),
                                    new Creater("BetaTaskData", 7, () => new BetaTaskData {IsProcess = true}),
                                    new Creater("DeltaTaskData", 0, () => new DeltaTaskData())
                );
            foreach(var deltaTaskId in addTasksInfo["DeltaTaskData"].Ids)
                WaitTaskState(deltaTaskId, TaskState.Finished);
            foreach(var betaTaskId in addTasksInfo["BetaTaskData"].Ids)
                WaitTaskState(betaTaskId, TaskState.InProcess);
            tasksListPage = LoadTasksListPage();
        }

        public override void TearDown()
        {
            FinishBetaTasks(addTasksInfo["BetaTaskData"].Ids);
            base.TearDown();
        }

        [Test]
        public void SearchByEnqueTicksTest()
        {
            var taskInfos = GetAllTaskInfos().ToArray();

            var alfasEnqueueTime = addTasksInfo["AlphaTaskData"].AddTime;
            var betasEnqueueTime = addTasksInfo["BetaTaskData"].AddTime;
            var deltasEnqueueTime = addTasksInfo["DeltaTaskData"].AddTime;

            SearchByEnqueTicksTestBase(taskInfos, betasEnqueueTime, deltasEnqueueTime);
            SearchByEnqueTicksTestBase(taskInfos, alfasEnqueueTime, deltasEnqueueTime);
            SearchByEnqueTicksTestBase(taskInfos, betasEnqueueTime, null);
            SearchByEnqueTicksTestBase(taskInfos, null, betasEnqueueTime);
            SearchByEnqueTicksTestBase(taskInfos, maxTime, alfasEnqueueTime);
        }

        [Test]
        public void SearchByMinimalStartTicksTest()
        {
            var taskInfos = GetAllTaskInfos().ToArray();

            var alfasEnqueueTime = addTasksInfo["AlphaTaskData"].AddTime.AddSeconds(3600);
            var betasEnqueueTime = addTasksInfo["BetaTaskData"].AddTime.AddSeconds(5);
            var deltasEnqueueTime = addTasksInfo["DeltaTaskData"].AddTime.AddSeconds(1);

            SearchByMinimalStartTicksTestBase(taskInfos, betasEnqueueTime, alfasEnqueueTime);
            SearchByMinimalStartTicksTestBase(taskInfos, betasEnqueueTime, maxTime);
            SearchByMinimalStartTicksTestBase(taskInfos, alfasEnqueueTime, null);
            SearchByMinimalStartTicksTestBase(taskInfos, maxTime, null);
            SearchByMinimalStartTicksTestBase(taskInfos, null, deltasEnqueueTime);
        }

        private IEnumerable<TaskInfo> GetAllTaskInfos()
        {
            var taskInfos = new List<TaskInfo>();
            var last = tasksCount;
            tasksListPage = tasksListPage.SearchUntilTaskListItemsCountIs(tasksCount);

            while(true)
            {
                var pageTasksCount = Math.Min(pageSize, last);
                tasksListPage = tasksListPage.RefreshUntilTaskRowIsPresent(pageTasksCount);
                taskInfos.AddRange(tasksListPage.GetTaskInfos(pageTasksCount));

                last -= pageTasksCount;
                if(last <= 0)
                    break;
                tasksListPage = tasksListPage.GoToNextPage();
            }
            return taskInfos;
        }

        private DateTime RoundDateTimeSeconds(DateTime datetime)
        {
            return new DateTime(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second, DateTimeKind.Utc);
        }

        private void SearchByEnqueTicksTestBase(IEnumerable<TaskInfo> taskInfos, DateTime? fromTime, DateTime? toTime)
        {
            SimpleTestBase(taskInfos, fromTime, toTime, x => x.EnqueueTime, CheckTaskSearch);
        }

        private void SearchByMinimalStartTicksTestBase(IEnumerable<TaskInfo> taskInfos, DateTime? fromTime, DateTime? toTime)
        {
            SimpleTestBase(taskInfos, fromTime, toTime, x => x.MinimalStartTime, CheckTaskSearchMinStartTicks);
        }

        private void SimpleTestBase(IEnumerable<TaskInfo> taskInfos, DateTime? fromTime, DateTime? toTime, Func<TaskInfo, DateTime?> pathToTime, Action<string[], DateTime?, DateTime?> checkMethod)
        {
            Func<DateTime, DateTime> transform = x => RoundDateTimeSeconds(x).Subtract(TimeSpan.FromMilliseconds(50));

            var lowerBound = transform(fromTime ?? new DateTime(2012, 11, 11));
            var upperBound = transform(toTime ?? maxTime);
            var expectedIds = taskInfos.Where(x => pathToTime(x) <= upperBound && pathToTime(x) >= lowerBound).Select(x => x.TaskId);
            checkMethod(expectedIds.ToArray(), fromTime, toTime);
        }

        private void CheckTaskSearchMinStartTicks(string[] ids, DateTime? fromTime, DateTime? toTime)
        {
            tasksListPage.ShowPanel.ClickAndWaitAnimation();
            tasksListPage.MinimalStartTicksDateFrom.WaitVisibleWithRetries();
            tasksListPage.MinimalStartTicksTimeFrom.WaitVisibleWithRetries();
            tasksListPage.MinimalStartTicksDateTo.WaitVisibleWithRetries();
            tasksListPage.MinimalStartTicksTimeTo.WaitVisibleWithRetries();
            TasksListPage.SetDateTime(tasksListPage.MinimalStartTicksDateFrom, tasksListPage.MinimalStartTicksTimeFrom, fromTime);
            TasksListPage.SetDateTime(tasksListPage.MinimalStartTicksDateTo, tasksListPage.MinimalStartTicksTimeTo, toTime);
            DoCheck(ref tasksListPage, ids);
        }

        private void CheckTaskSearch(string[] ids, DateTime? fromTime, DateTime? toTime)
        {
            tasksListPage.ShowPanel.ClickAndWaitAnimation();
            tasksListPage.TicksDateFrom.WaitVisibleWithRetries();
            tasksListPage.TicksTimeFrom.WaitVisibleWithRetries();
            tasksListPage.TicksDateTo.WaitVisibleWithRetries();
            tasksListPage.TicksTimeTo.WaitVisibleWithRetries();
            TasksListPage.SetDateTime(tasksListPage.TicksDateFrom, tasksListPage.TicksTimeFrom, fromTime);
            TasksListPage.SetDateTime(tasksListPage.TicksDateTo, tasksListPage.TicksTimeTo, toTime);
            DoCheck(ref tasksListPage, ids);
        }

        private readonly DateTime maxTime = new DateTime(2020, 12, 31);

        private Dictionary<string, AddTaskInfo> addTasksInfo;
        private TasksListPage tasksListPage;
        private const int pageSize = 100;
        private const int tasksCount = 21;
    }
}