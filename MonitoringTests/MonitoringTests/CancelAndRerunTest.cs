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

        [Test]
        [Repeat(10)]
        public void CancelAndRerunTaskTest()
        {
            var taskId = remoteTaskQueue.CreateTask(new AlphaTaskData()).Queue(TimeSpan.FromHours(3));
            WaitTaskState(taskId, TaskState.New);
            var taskListPage = LoadTasksListPage();
            taskListPage = taskListPage.RefreshUntilTaskRowIsPresent(1);
            taskListPage = taskListPage.RefreshUntilState(0, "New");

            taskListPage = taskListPage.CancelTask(0);
            WaitTaskState(taskId, TaskState.Canceled);
            taskListPage = taskListPage.RefreshUntilState(0, "Canceled");

            taskListPage = taskListPage.RerunTask(0);
            WaitTaskState(taskId, TaskState.Finished);
            taskListPage.RefreshUntilState(0, "Finished");
        }

        private IRemoteTaskQueue remoteTaskQueue;
    }
}