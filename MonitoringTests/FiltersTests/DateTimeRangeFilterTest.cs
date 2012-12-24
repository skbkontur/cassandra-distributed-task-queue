using System;
using System.Collections.Generic;
using System.Threading;

using NUnit.Framework;

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
                    new Creater("AlphaTaskData", 11, () => new AlphaTaskData()),
                    new Creater("BetaTaskData", 7, () => new BetaTaskData()),
                    new Creater("DeltaTaskData", 3, () => new DeltaTaskData())
                );
            CreateUser("user", "psw");
            Thread.Sleep(11000);
            tasksListPage = Login("user", "psw");
        }

        [Test]
        public void SearchOnTicksTest()
        {
            CheckTaskSearch(addTasksInfo["AlphaTaskData"], addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["BetaTaskData"], addTasksInfo["DeltaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"], DateTime.UtcNow);

            CheckTaskSearch(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"]), addTasksInfo["DeltaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"]), DateTime.UtcNow);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"])), DateTime.UtcNow);

            CheckTaskSearch(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"]), null);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"], null);
            CheckTaskSearch(new AddTaskInfo(new List<string>(), DateTime.UtcNow), null);

            CheckTaskSearch(new AddTaskInfo(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"]).Ids, null), addTasksInfo["DeltaTaskData"].AddTime);
            CheckTaskSearch(new AddTaskInfo(addTasksInfo["AlphaTaskData"].Ids, null), addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearch(new AddTaskInfo(new List<string>(), null), addTasksInfo["AlphaTaskData"].AddTime);
        }

        [Test]
        public void SearchOnMinimalStartTicksTest()
        {
            addTasksInfo["AlphaTaskData"].AddTime = addTasksInfo["AlphaTaskData"].AddTime.Value.AddSeconds(11);
            addTasksInfo["BetaTaskData"].AddTime = addTasksInfo["BetaTaskData"].AddTime.Value.AddSeconds(7);
            addTasksInfo["DeltaTaskData"].AddTime = addTasksInfo["DeltaTaskData"].AddTime.Value.AddSeconds(3);

            CheckTaskSearchMinStartTicks(addTasksInfo["AlphaTaskData"], DateTime.UtcNow);
            CheckTaskSearchMinStartTicks(addTasksInfo["BetaTaskData"], addTasksInfo["AlphaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks( addTasksInfo["DeltaTaskData"], addTasksInfo["BetaTaskData"].AddTime);

            CheckTaskSearchMinStartTicks(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"]), DateTime.UtcNow);
            CheckTaskSearchMinStartTicks(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"]), addTasksInfo["AlphaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"])), DateTime.UtcNow);

            CheckTaskSearchMinStartTicks(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"]), null);
            CheckTaskSearchMinStartTicks(addTasksInfo["AlphaTaskData"], null);
            CheckTaskSearchMinStartTicks(new AddTaskInfo(new List<string>(), DateTime.UtcNow), null);

            CheckTaskSearchMinStartTicks(new AddTaskInfo(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"]).Ids, null), addTasksInfo["AlphaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks(new AddTaskInfo(addTasksInfo["DeltaTaskData"].Ids, null), addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks(new AddTaskInfo(new List<string>(), null), addTasksInfo["DeltaTaskData"].AddTime);
        }

        private void CheckTaskSearchMinStartTicks(AddTaskInfo addTaskInfo, DateTime? toTime)
        {
            TasksListPage.SetDateTime(tasksListPage.MinimalStartTicksDateFrom, tasksListPage.MinimalStartTicksTimeFrom, addTaskInfo.AddTime);
            TasksListPage.SetDateTime(tasksListPage.MinimalStartTicksDateTo, tasksListPage.MinimalStartTicksTimeTo, toTime);
            tasksListPage = tasksListPage.SearchTasks();
            DoCheck(tasksListPage, addTaskInfo);
        }

        private void CheckTaskSearch(AddTaskInfo addTaskInfo, DateTime? toTime)
        {
            TasksListPage.SetDateTime(tasksListPage.TicksDateFrom, tasksListPage.TicksTimeFrom, addTaskInfo.AddTime);
            TasksListPage.SetDateTime(tasksListPage.TicksDateTo, tasksListPage.TicksTimeTo, toTime);
            tasksListPage = tasksListPage.SearchTasks();
            DoCheck(tasksListPage, addTaskInfo);
        }



        private Dictionary<string, AddTaskInfo> addTasksInfo;
        private TasksListPage tasksListPage;
    }
}