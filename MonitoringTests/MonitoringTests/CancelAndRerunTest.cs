using System;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.MonitoringTests
{
    public class CancelAndRerunTest : FiltersTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            remoteTaskQueue = container.Get<IRemoteTaskQueue>();
        }

        [Repeat(3)]
        [Test]
        public void CancelAndRerunTaskTest()
        {
            CreateUser("user", "psw");
            var taskId = remoteTaskQueue.CreateTask(new AlphaTaskData()).Queue(TimeSpan.FromHours(3));
            var taskListPage = Login("user", "psw");
            taskListPage = taskListPage.RefreshUntilTaskRowIsPresent(1);
            taskListPage = taskListPage.RefreshUntilState(0, "New");

            taskListPage = taskListPage.CancelTask(0);
            WaitTaskState(taskId, TaskState.Canceled);
            taskListPage = taskListPage.RefreshUntilState(0, "Canceled");

            taskListPage = taskListPage.Refresh();

            taskListPage = taskListPage.RerunTask(0);
            WaitTaskState(taskId, TaskState.Finished);
            taskListPage.RefreshUntilState(0, "Finished");
        }

        private IRemoteTaskQueue remoteTaskQueue;
    }
}