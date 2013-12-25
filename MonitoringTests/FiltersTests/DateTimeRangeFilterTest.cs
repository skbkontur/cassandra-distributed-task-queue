using System;
using System.Collections.Generic;

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
            addTasksInfo = AddTasks(30,
                    new Creater("AlphaTaskData", 3600, () => new AlphaTaskData()),
                    new Creater("BetaTaskData", 5, () => new BetaTaskData{ IsProcess = true}),
                    new Creater("DeltaTaskData", 1, () => new DeltaTaskData())
                );
            CreateUser("user", "psw");
            foreach(var deltaTaskId in addTasksInfo["DeltaTaskData"].Ids)
                WaitTaskState(deltaTaskId, TaskState.Finished);
            foreach(var betaTaskId in addTasksInfo["BetaTaskData"].Ids)
                WaitTaskState(betaTaskId, TaskState.InProcess);
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

        [Repeat(10)]
        [Test]
        public void SearchOnTicksTest()
        {
            var maxTime = new DateTime(2020, 12, 31);

            CheckTaskSearch(addTasksInfo["AlphaTaskData"], addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["BetaTaskData"], addTasksInfo["DeltaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"], maxTime);

            CheckTaskSearch(addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"]), addTasksInfo["DeltaTaskData"].AddTime);
            CheckTaskSearch(addTasksInfo["BetaTaskData"].Add(addTasksInfo["DeltaTaskData"]), maxTime);
            CheckTaskSearch((addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"])).Add(addTasksInfo["DeltaTaskData"]), maxTime);

            CheckTaskSearch(addTasksInfo["BetaTaskData"].Add(addTasksInfo["DeltaTaskData"]), null);
            CheckTaskSearch(addTasksInfo["DeltaTaskData"], null);
            CheckTaskSearch(new AddTaskInfo(new List<string>(), DateTime.UtcNow), null);

            var revAlpha = addTasksInfo["AlphaTaskData"].Ids.ToArray();
            Array.Reverse(revAlpha);
            var revBeta = addTasksInfo["BetaTaskData"].Ids.ToArray();
            Array.Reverse(revBeta);

            var list = new List<string>();
            list.AddRange(revBeta);
            list.AddRange(revAlpha);
            CheckTaskSearch(new AddTaskInfo(list, null), addTasksInfo["DeltaTaskData"].AddTime);
            list.Clear();
            list.AddRange(revAlpha);
            CheckTaskSearch(new AddTaskInfo(list, null), addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearch(new AddTaskInfo(new List<string>(), null), addTasksInfo["AlphaTaskData"].AddTime);
        }

        [Repeat(10)]
        [Test]
        public void SearchOnMinimalStartTicksTest()
        {
            var maxTime = new DateTime(2020, 12, 31);

            addTasksInfo["AlphaTaskData"].AddTime = addTasksInfo["AlphaTaskData"].AddTime.Value.AddSeconds(3600);
            addTasksInfo["BetaTaskData"].AddTime = addTasksInfo["BetaTaskData"].AddTime.Value.AddSeconds(5);
            addTasksInfo["DeltaTaskData"].AddTime = addTasksInfo["DeltaTaskData"].AddTime.Value.AddSeconds(1);

            CheckTaskSearchMinStartTicks(addTasksInfo["AlphaTaskData"], maxTime);
            CheckTaskSearchMinStartTicks(addTasksInfo["BetaTaskData"], addTasksInfo["AlphaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks(addTasksInfo["DeltaTaskData"], addTasksInfo["BetaTaskData"].AddTime);

            CheckTaskSearchMinStartTicks(addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"]), maxTime);
            CheckTaskSearchMinStartTicks(addTasksInfo["BetaTaskData"].Add(addTasksInfo["DeltaTaskData"]), addTasksInfo["AlphaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks((addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"])).Add(addTasksInfo["DeltaTaskData"]), maxTime);

            CheckTaskSearchMinStartTicks(addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"]), null);
            CheckTaskSearchMinStartTicks(addTasksInfo["AlphaTaskData"], null);
            CheckTaskSearchMinStartTicks(new AddTaskInfo(new List<string>(), maxTime), null);

            var revDelta = addTasksInfo["DeltaTaskData"].Ids.ToArray();
            Array.Reverse(revDelta);
            var revBeta = addTasksInfo["BetaTaskData"].Ids.ToArray();
            Array.Reverse(revBeta);

            var list = new List<string>();
            list.AddRange(revDelta);
            list.AddRange(revBeta);
            CheckTaskSearchMinStartTicks(new AddTaskInfo(list, null), addTasksInfo["AlphaTaskData"].AddTime);
            list.Clear();
            list.AddRange(revDelta);
            CheckTaskSearchMinStartTicks(new AddTaskInfo(list, null), addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks(new AddTaskInfo(new List<string>(), null), addTasksInfo["DeltaTaskData"].AddTime);
        }

        private void CheckTaskSearchMinStartTicks(AddTaskInfo addTaskInfo, DateTime? toTime)
        {
            tasksListPage.ShowPanel.ClickAndWaitAnimation();
            TasksListPage.SetDateTime(tasksListPage.MinimalStartTicksDateFrom, tasksListPage.MinimalStartTicksTimeFrom, addTaskInfo.AddTime);
            TasksListPage.SetDateTime(tasksListPage.MinimalStartTicksDateTo, tasksListPage.MinimalStartTicksTimeTo, toTime);
            DoCheck(ref tasksListPage, addTaskInfo);
        }

        private void CheckTaskSearch(AddTaskInfo addTaskInfo, DateTime? toTime)
        {
            tasksListPage.ShowPanel.ClickAndWaitAnimation();
            TasksListPage.SetDateTime(tasksListPage.TicksDateFrom, tasksListPage.TicksTimeFrom, addTaskInfo.AddTime);
            TasksListPage.SetDateTime(tasksListPage.TicksDateTo, tasksListPage.TicksTimeTo, toTime);
            DoCheck(ref tasksListPage, addTaskInfo);
        }

        private Dictionary<string, AddTaskInfo> addTasksInfo;
        private TasksListPage tasksListPage;
    }
}