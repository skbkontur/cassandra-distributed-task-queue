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
                    new Creater("AlphaTaskData", 3600, () => new AlphaTaskData()),
                    new Creater("BetaTaskData", 11, () => new BetaTaskData{ IsProcess = true}),
                    new Creater("DeltaTaskData", 1, () => new DeltaTaskData())
                );
            CreateUser("user", "psw");
            Thread.Sleep(15000);
            tasksListPage = Login("user", "psw");
        }

        public override void TearDown()
        {
            foreach (var betaId in addTasksInfo["BetaTaskData"].Ids)
            {
                var task = handleTaskCollection.GetTask(betaId);
                var data = serializer.Deserialize<BetaTaskData>(task.Data);
                data.IsProcess = false;
                task.Data = serializer.Serialize(data);
                try
                {
                    handleTaskCollection.AddTask(task);
                }
                catch (Exception e)
                {
                }
            }
            base.TearDown();
        }

        [Test]
        public void SearchOnTicksTest()
        {
            var maxTime = new DateTime(2020, 12, 31);

            CheckTaskSearch(addTasksInfo["AlphaTaskData"], addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["BetaTaskData"], addTasksInfo["DeltaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"], maxTime);

            CheckTaskSearch(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"]), addTasksInfo["DeltaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"]), maxTime);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"])), maxTime);

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
            var maxTime = new DateTime(2020, 12, 31);

            addTasksInfo["AlphaTaskData"].AddTime = addTasksInfo["AlphaTaskData"].AddTime.Value.AddSeconds(3600);
            addTasksInfo["BetaTaskData"].AddTime = addTasksInfo["BetaTaskData"].AddTime.Value.AddSeconds(11);
            addTasksInfo["DeltaTaskData"].AddTime = addTasksInfo["DeltaTaskData"].AddTime.Value.AddSeconds(1);

            CheckTaskSearchMinStartTicks(addTasksInfo["AlphaTaskData"], maxTime);
            CheckTaskSearchMinStartTicks(addTasksInfo["BetaTaskData"], addTasksInfo["AlphaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks( addTasksInfo["DeltaTaskData"], addTasksInfo["BetaTaskData"].AddTime);

            CheckTaskSearchMinStartTicks(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"]), maxTime);
            CheckTaskSearchMinStartTicks(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"]), addTasksInfo["AlphaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks(addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"])), maxTime);

            CheckTaskSearchMinStartTicks(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"]), null);
            CheckTaskSearchMinStartTicks(addTasksInfo["AlphaTaskData"], null);
            CheckTaskSearchMinStartTicks(new AddTaskInfo(new List<string>(), maxTime), null);

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