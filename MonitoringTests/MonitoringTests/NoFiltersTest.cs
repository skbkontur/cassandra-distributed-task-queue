using System;
using System.Collections.Generic;

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
            var expectedIds = new List<string>();
            for(var i = 0; i < 10; i++)
                expectedIds.Add(AddTask(new SimpleTaskData()));
            foreach(var id in expectedIds)
                Console.WriteLine(id);
            var tasksListPage = LoadTasksListPage().RefreshUntilAllTasksInState(10, "Finished");
            Console.WriteLine();
            var actualIds = new List<string>();
            for(var i = 0; i < 10; i++)
            {
                var item = tasksListPage.GetTaskListItem(i);
                item.TaskState.WaitText("Finished");
                item.TaskName.WaitText("SimpleTaskData");
                item.Attempts.WaitText("1");
                var taskId = item.TaskId.GetText();
                actualIds.Add(taskId);
                var details = tasksListPage.GoToTaskDetails(i);
                details.TaskId.WaitText(taskId);
                details.TaskState.WaitText("Finished");
                details.TaskName.WaitText("SimpleTaskData");
                details.Attempts.WaitText("1");
                tasksListPage = details.GoToTasksListPage();
            }
            Assert.That(actualIds, Is.EquivalentTo(expectedIds));
        }

        [Test]
        public void EmptyListTest()
        {
            var tasksListPage = LoadTasksListPage();
            tasksListPage.GetTaskListItem(0).WaitAbsence();
        }

        private string AddTask<T>(T taskData) where T : ITaskData
        {
            return remoteTaskQueue.CreateTask(taskData).Queue();
        }

        private IRemoteTaskQueue remoteTaskQueue;
    }
}