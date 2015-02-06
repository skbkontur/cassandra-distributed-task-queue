using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class TaskIdFiltrTest : FiltersTestBase
    {
        [Test]
        public void SearchOnTaskIdTest()
        {
            var tasksIds = AddTasks(2,
                                    new Creater("AlphaTaskData", 0, () => new AlphaTaskData()),
                                    new Creater("BetaTaskData", 0, () => new BetaTaskData()),
                                    new Creater("DeltaTaskData", 0, () => new DeltaTaskData())
                ).Select(x => x.Value.Ids).Aggregate((x, y) => new List<string>(x.Concat(y)));
            var tasksListPage = LoadTasksListPage();
            foreach(var taskId in tasksIds)
            {
                tasksListPage.ShowPanel.ClickAndWaitAnimation();
                tasksListPage.TaskId.WaitVisibleWithRetries();
                tasksListPage.TaskId.WaitEnabledWithRetries();
                tasksListPage.TaskId.SetValue(taskId);
                tasksListPage = tasksListPage.SearchTasks();
                DoCheck(ref tasksListPage, new[] {taskId});
            }
        }
    }
}