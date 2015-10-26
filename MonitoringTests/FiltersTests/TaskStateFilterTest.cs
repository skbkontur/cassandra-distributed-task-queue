using System.Collections.Generic;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

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
        public void SearchByStateTest()
        {
            CheckTaskSearch(new[] {tasksListPage.New}, addTasksInfo["AlphaTaskData"]);
            CheckTaskSearch(new[] {tasksListPage.InProcess}, addTasksInfo["BetaTaskData"]);
            CheckTaskSearch(new[] {tasksListPage.Finished}, addTasksInfo["DeltaTaskData"]);
            CheckTaskSearch(new[] {tasksListPage.New, tasksListPage.InProcess}, addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"]));
            CheckTaskSearch(new[] {tasksListPage.New, tasksListPage.Finished}, addTasksInfo["AlphaTaskData"].Add(addTasksInfo["DeltaTaskData"]));
            CheckTaskSearch(new[] {tasksListPage.InProcess, tasksListPage.Finished}, addTasksInfo["BetaTaskData"].Add(addTasksInfo["DeltaTaskData"]));
            CheckTaskSearch(new[] {tasksListPage.New, tasksListPage.InProcess, tasksListPage.Finished},
                            addTasksInfo["AlphaTaskData"].Add(addTasksInfo["BetaTaskData"]).Add(addTasksInfo["DeltaTaskData"]));
        }

        private void CheckTaskSearch(CheckBox[] checkBoxes, AddTaskInfo addTaskInfo)
        {
            tasksListPage.ShowPanel.ClickAndWaitAnimation();
            foreach(var checkBox in checkBoxes)
                checkBox.CheckAndWait();
            tasksListPage = tasksListPage.SearchTasks();
            DoCheck(ref tasksListPage, addTaskInfo.Ids.ToArray());
            tasksListPage.ShowPanel.ClickAndWaitAnimation();
            foreach(var checkBox in checkBoxes)
                checkBox.UncheckAndWait();
            tasksListPage = tasksListPage.SearchTasks();
        }

        private Dictionary<string, AddTaskInfo> addTasksInfo;
        private TasksListPage tasksListPage;
    }
}