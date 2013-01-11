using System;
using System.Threading;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.MonitoringTests
{
    public class CancelAndRerunTest : FiltersTestBase
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
            var taskId = remoteTaskQueue.CreateTask(new AlphaTaskData()).Queue(TimeSpan.FromHours(3));
            var taskListPage = Login("user", "psw");
            taskListPage.CheckTaskListItemsCount(1);
            taskListPage.GetTaskListItem(0).TaskState.WaitText("New");
            taskListPage.GetTaskListItem(0).CancelTask();
            WaitTaskState(taskId, TaskState.Canceled);
            taskListPage = taskListPage.GoTo<TasksListPage>();
            taskListPage = taskListPage.Refresh();
            taskListPage.GetTaskListItem(0).TaskState.WaitText("Canceled");
            taskListPage.GetTaskListItem(0).RerunTask();
            WaitTaskState(taskId, TaskState.Finished);
            taskListPage = taskListPage.GoTo<TasksListPage>();
            taskListPage = taskListPage.SearchTasks();
            taskListPage.GetTaskListItem(0).TaskState.WaitText("Finished");
        }
    }
}