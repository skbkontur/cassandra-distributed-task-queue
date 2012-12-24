using System.Collections.Generic;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class NameFiltrTest : FiltersTestBase
    {
        [Test]
        public void NameSearchTest()
        {
            var alphaTasksInfo = AddTasks(7, new Creater("AlphaTaskData", 0, () => new AlphaTaskData()));
            var betaTasksInfo = AddTasks(5, new Creater("BetaTaskData", 0, () => new BetaTaskData()));
            var deltaTasksInfo = AddTasks(3, new Creater("DeltaTaskData", 0, () => new DeltaTaskData()));

            var expected = new Dictionary<string, AddTaskInfo>
                {
                    {"AlphaTaskData", alphaTasksInfo["AlphaTaskData"]},
                    {"BetaTaskData", betaTasksInfo["BetaTaskData"]},
                    {"DeltaTaskData", deltaTasksInfo["DeltaTaskData"]}
                };

            CreateUser("pasan", "psn");
            var tasksListPage = Login("pasan", "psn");

            var taskNames = new[] {"AlphaTaskData", "BetaTaskData", "DeltaTaskData"};
            foreach(var taskName in taskNames)
            {
                tasksListPage.TaskName.SetSelectedText(taskName);
                tasksListPage = tasksListPage.SearchTasks();
                DoCheck(tasksListPage, expected[taskName]);
            }
        }
    }
}