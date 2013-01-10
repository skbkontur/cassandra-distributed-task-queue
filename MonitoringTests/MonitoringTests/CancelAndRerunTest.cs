using System;
using System.Threading;

using NUnit.Framework;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.MonitoringTests
{
    public class CancelAndRerunTest : MonitoringFunctionalTestBase
    {
        private IRemoteTaskQueue remoteTaskQueue;

        public override void SetUp()
        {
            base.SetUp();
            remoteTaskQueue = container.Get<IRemoteTaskQueue>();
        }

        [Test]
        public void CancelAndRerunTaskTest()
        {
            CreateUser("user", "psw");
            remoteTaskQueue.Queue(new AlphaTaskData(), TimeSpan.FromHours(3));
            var taskListPage = Login("user", "psw");
            taskListPage.CheckTaskListItemsCount(1);
            taskListPage.GetTaskListItem(0).TaskState.WaitText("New");
            taskListPage.GetTaskListItem(0).CancelTask();
            Thread.Sleep(1000);
            taskListPage = taskListPage.GoTo<TasksListPage>();
            taskListPage = taskListPage.Refresh();
            taskListPage.GetTaskListItem(0).TaskState.WaitText("Canceled");
            taskListPage.GetTaskListItem(0).RerunTask();
            Thread.Sleep(1000);
            taskListPage = taskListPage.GoTo<TasksListPage>();
            taskListPage = taskListPage.SearchTasks();
            taskListPage.GetTaskListItem(0).TaskState.WaitText("Finished");
        }
    }
}