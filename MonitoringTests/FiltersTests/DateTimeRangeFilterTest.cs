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
            addTasksInfo = AddTasks(70,
                    new Creater("AlphaTaskData", 3600, () => new AlphaTaskData()),
                    new Creater("BetaTaskData", 7, () => new BetaTaskData{ IsProcess = true}),
                    new Creater("DeltaTaskData", 0, () => new DeltaTaskData())
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

        [Test, Repeat(7)]
        public void SearchOnTicksTest()
        {
            var maxTime = new DateTime(2020, 12, 31);

            var expectedTasksInfo = addTasksInfo["AlphaTaskData"];
            CheckTaskSearch(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, addTasksInfo["BetaTaskData"].AddTime);

            expectedTasksInfo = addTasksInfo["BetaTaskData"];
            CheckTaskSearch(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, addTasksInfo["DeltaTaskData"].AddTime);

            expectedTasksInfo = addTasksInfo["DeltaTaskData"];
            CheckTaskSearch(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, maxTime);

            expectedTasksInfo = addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"]);
            CheckTaskSearch(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, addTasksInfo["DeltaTaskData"].AddTime);

            expectedTasksInfo = addTasksInfo["BetaTaskData"].Add(addTasksInfo["DeltaTaskData"]);
            CheckTaskSearch(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, maxTime);

            expectedTasksInfo = (addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"])).Add(addTasksInfo["DeltaTaskData"]);
            CheckTaskSearch(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, maxTime);

            expectedTasksInfo = addTasksInfo["BetaTaskData"].Add(addTasksInfo["DeltaTaskData"]);
            CheckTaskSearch(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, null);

            expectedTasksInfo = addTasksInfo["DeltaTaskData"];
            CheckTaskSearch(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, null);

            CheckTaskSearch(new string[0], DateTime.UtcNow, null);

            var revAlpha = addTasksInfo["AlphaTaskData"].Ids.ToArray();
            Array.Reverse(revAlpha);
            var revBeta = addTasksInfo["BetaTaskData"].Ids.ToArray();
            Array.Reverse(revBeta);

            var list = new List<string>();
            list.AddRange(revBeta);
            list.AddRange(revAlpha);
            CheckTaskSearch(list.ToArray(), null, addTasksInfo["DeltaTaskData"].AddTime);
            list.Clear();
            list.AddRange(revAlpha);
            CheckTaskSearch(list.ToArray(), null, addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearch(new string[0], null, addTasksInfo["AlphaTaskData"].AddTime);
        }

        [Test, Repeat(7)]
        public void SearchOnMinimalStartTicksTest()
        {
            var maxTime = new DateTime(2020, 12, 31);

            addTasksInfo["AlphaTaskData"].AddTime = addTasksInfo["AlphaTaskData"].AddTime.AddSeconds(3600);
            addTasksInfo["BetaTaskData"].AddTime = addTasksInfo["BetaTaskData"].AddTime.AddSeconds(5);
            addTasksInfo["DeltaTaskData"].AddTime = addTasksInfo["DeltaTaskData"].AddTime.AddSeconds(1);

            var expectedTasksInfo = addTasksInfo["AlphaTaskData"];
            CheckTaskSearchMinStartTicks(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, maxTime);

            expectedTasksInfo = addTasksInfo["BetaTaskData"];
            CheckTaskSearchMinStartTicks(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, addTasksInfo["AlphaTaskData"].AddTime);

            expectedTasksInfo = addTasksInfo["DeltaTaskData"];
            CheckTaskSearchMinStartTicks(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, addTasksInfo["BetaTaskData"].AddTime);

            expectedTasksInfo = addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"]);
            CheckTaskSearchMinStartTicks(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, maxTime);

            expectedTasksInfo = addTasksInfo["BetaTaskData"].Add(addTasksInfo["DeltaTaskData"]);
            CheckTaskSearchMinStartTicks(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, addTasksInfo["AlphaTaskData"].AddTime);

            expectedTasksInfo = (addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"])).Add(addTasksInfo["DeltaTaskData"]);
            CheckTaskSearchMinStartTicks(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, maxTime);

            expectedTasksInfo = addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"]);
            CheckTaskSearchMinStartTicks(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, null);

            expectedTasksInfo = addTasksInfo["AlphaTaskData"];
            CheckTaskSearchMinStartTicks(expectedTasksInfo.Ids.ToArray(), expectedTasksInfo.AddTime, null);

            CheckTaskSearchMinStartTicks(new string[0], maxTime, null);

            var revDelta = addTasksInfo["DeltaTaskData"].Ids.ToArray();
            Array.Reverse(revDelta);
            var revBeta = addTasksInfo["BetaTaskData"].Ids.ToArray();
            Array.Reverse(revBeta);

            var list = new List<string>();
            list.AddRange(revDelta);
            list.AddRange(revBeta);
            CheckTaskSearchMinStartTicks(list.ToArray(), null, addTasksInfo["AlphaTaskData"].AddTime);
            list.Clear();
            list.AddRange(revDelta);
            CheckTaskSearchMinStartTicks(list.ToArray(), null, addTasksInfo["BetaTaskData"].AddTime);
            CheckTaskSearchMinStartTicks(new string[0], null, addTasksInfo["DeltaTaskData"].AddTime);
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
            TasksListPage.SetDateTime(tasksListPage.TicksDateFrom, tasksListPage.TicksTimeFrom, fromTime);
            TasksListPage.SetDateTime(tasksListPage.TicksDateTo, tasksListPage.TicksTimeTo, toTime);
            DoCheck(ref tasksListPage, ids);
        }

        private Dictionary<string, AddTaskInfo> addTasksInfo;
        private TasksListPage tasksListPage;
    }
}