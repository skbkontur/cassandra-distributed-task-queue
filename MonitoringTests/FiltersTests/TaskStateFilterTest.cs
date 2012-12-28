using System;
using System.Collections.Generic;
using System.Threading;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;
using SKBKontur.Catalogue.WebTestCore.SystemControls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class TaskStateFilterTest : FiltersTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            addTasksInfo = AddTasks(7,
                                    new Creater("AlphaTaskData", 3600, () => new AlphaTaskData()),
                                    new Creater("BetaTaskData", 4, () => new BetaTaskData {IsProcess = true}),
                                    new Creater("DeltaTaskData", 1, () => new DeltaTaskData())
                );
            CreateUser("user", "psw");
            Thread.Sleep(4000);
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
        public void SearchByStateTest()
        {
            CheckTaskSearch(new []{ tasksListPage.New }, addTasksInfo["AlphaTaskData"]);
            CheckTaskSearch(new []{ tasksListPage.InProcess }, addTasksInfo["BetaTaskData"]);
            CheckTaskSearch(new []{ tasksListPage.Finished }, addTasksInfo["DeltaTaskData"]);
            CheckTaskSearch(new []{ tasksListPage.New, tasksListPage.InProcess}, addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"]));
            CheckTaskSearch(new []{ tasksListPage.New, tasksListPage.Finished}, addTasksInfo["DeltaTaskData"].Add(addTasksInfo["AlphaTaskData"]));
            CheckTaskSearch(new []{ tasksListPage.InProcess, tasksListPage.Finished}, addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"]));
            CheckTaskSearch(new []{ tasksListPage.New, tasksListPage.InProcess, tasksListPage.Finished},
                addTasksInfo["DeltaTaskData"].Add(addTasksInfo["BetaTaskData"].Add(addTasksInfo["AlphaTaskData"])));
        }

        private void CheckTaskSearch(CheckBox [] checkBoxes, AddTaskInfo addTaskInfo)
        {
            foreach(var checkBox in checkBoxes)
                checkBox.CheckAndWait();
            tasksListPage = tasksListPage.SearchTasks();
            DoCheck(tasksListPage, addTaskInfo);
            foreach(var checkBox in checkBoxes)
                checkBox.UncheckAndWait();
            tasksListPage = tasksListPage.SearchTasks();
        }

        private Dictionary<string, AddTaskInfo> addTasksInfo;
        private TasksListPage tasksListPage;
    }
}