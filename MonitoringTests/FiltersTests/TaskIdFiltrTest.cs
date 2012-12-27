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
            var tasksIds = AddTasks(7,
                     new Creater("AlphaTaskData", 0, () => new AlphaTaskData()),
                     new Creater("BetaTaskData", 0, () => new BetaTaskData()),
                     new Creater("DeltaTaskData", 0, () => new DeltaTaskData())
                 ).Select(x => x.Value.Ids).Aggregate((x, y) => new List<string>(x.Concat(y)));
            CreateUser("user", "psw");
            var tasksListPage = Login("user", "psw");
            foreach(var taskId in tasksIds)
            {
                tasksListPage.TaskId.SetValue(taskId);
                tasksListPage = tasksListPage.SearchTasks();
                DoCheck(tasksListPage, new AddTaskInfo(new List<string>{taskId}, null));
            }
        }
    }
}