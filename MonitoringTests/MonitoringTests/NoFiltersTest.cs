using System;

using NUnit.Framework;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.MonitoringTests
{
    public class NoFiltersTest : MonitoringFunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            remoteTaskQueue = container.Get<IRemoteTaskQueue>();
        }

        [Test]
        public void Test()
        {
            CreateUser("user", "psw");
            var ids = new string[10];
            for(int i = 0; i < 10; i++)
                ids[i] = AddTask(new SimpleTaskData());
            foreach(var id in ids)
                Console.WriteLine(id);
            var tasksListPage = Login("user", "psw");
            tasksListPage.CheckTaskListItemsCount(10);
            Console.WriteLine();
            for(int i = 0; i < 10; i++)
            {
                var item = tasksListPage.GetTaskListItem(i);
                item.TaskId.WaitText(ids[9 - i]);
                item.TaskState.WaitText("Finished");
                item.TaskName.WaitText("SimpleTaskData");
                item.Attempts.WaitText("1");
                var details = tasksListPage.GoToTaskDetails(i);
                details.TaskId.WaitText(ids[9 - i]);
                details.TaskState.WaitText("Finished");
                details.TaskName.WaitText("SimpleTaskData");
                details.Attempts.WaitText("1");
                tasksListPage = details.GoToTasksListPage();
            }
        }

        private string AddTask<T>(T taskData) where T : ITaskData
        {
            return remoteTaskQueue.Queue(taskData);
        }

        private IRemoteTaskQueue remoteTaskQueue;
    }
}