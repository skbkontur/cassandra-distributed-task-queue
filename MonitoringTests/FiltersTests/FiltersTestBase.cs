using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GroBuf;

using MoreLinq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class FiltersTestBase : MonitoringFunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            serializer = container.Get<ISerializer>();
            remoteTaskQueue = container.Get<IRemoteTaskQueue>();
            ((IRemoteTaskQueueBackdoor)remoteTaskQueue).ResetTicksHolderInMemoryState();
            taskDataStorage = container.Get<TaskDataStorage>();
            taskMetaStorage = container.Get<TaskMetaStorage>();
        }

        protected Dictionary<string, AddTaskInfo> AddTasks(int iteration, params Creater[] creaters)
        {
            var result = new Dictionary<string, AddTaskInfo>();
            foreach(var creater in creaters)
            {
                var dt = DateTime.UtcNow;
                var ids = new List<string>();
                for(var i = 0; i < iteration; i++)
                {
                    var remoteTask = remoteTaskQueue.CreateTask(creater.Create());
                    ids.Add(remoteTask.Id);
                    remoteTask.Queue(creater.Delay);
                }
                result.Add(creater.TaskName, new AddTaskInfo(ids, dt));
                Thread.Sleep(1000);
            }
            return result;
        }

        protected void WaitTaskState(string taskId, TaskState taskState)
        {
            var startTime = DateTime.UtcNow;
            while(true)
            {
                if(remoteTaskQueue.GetTaskInfo(taskId).Context.State == taskState)
                    return;
                if(DateTime.UtcNow.Subtract(startTime) > TimeSpan.FromSeconds(15))
                    throw new Exception("Таска не приняла ожидаемое состояние за 15 сек.");
                Thread.Sleep(50);
            }
        }

        protected static void DoCheck(ref TasksListPage tasksListPage, string[] ids)
        {
            var expectedIds = new HashSet<string>();
            var actualIds = new HashSet<string>();
            Array.ForEach(ids, x => expectedIds.Add(x));
            const int tasksPerPage = 100;
            var parts = ids.Batch(tasksPerPage, Enumerable.ToArray);
            int cnt = 0;
            tasksListPage = tasksListPage.SearchUntilTaskListItemsCountIs(ids.Length);

            foreach(var pageIds in parts)
            {
                tasksListPage = tasksListPage.RefreshUntilTaskRowIsPresent(pageIds.Length);
                cnt++;
                for(int i = 0; i < pageIds.Length; i++)
                {
                    var taskId = tasksListPage.GetTaskListItem(i).TaskId.GetText();

                    Assert.True(expectedIds.Contains(taskId), "Не ожиданная таска с id: {0}.", taskId);
                    actualIds.Add(taskId);
                }
                if(ids.Length > cnt * tasksPerPage)
                    tasksListPage = tasksListPage.GoToNextPage();
            }
            Assert.AreEqual(actualIds.Count, expectedIds.Count, "Ожидалось найти {0} тасок, а было найдено {1}.", expectedIds.Count, actualIds.Count);
        }

        protected void FinishBetaTasks(List<string> betaTaskIds)
        {
            var taskMetas = taskMetaStorage.Read(betaTaskIds.ToArray());
            var taskDatas = taskDataStorage.Read(taskMetas.Values.ToArray());
            foreach(var pair in taskDatas)
            {
                var taskData = serializer.Deserialize<BetaTaskData>(pair.Value);
                taskData.IsProcess = false;
                taskDataStorage.Overwrite(taskMetas[pair.Key], serializer.Serialize(taskData));
            }
            foreach(var betaTaskId in betaTaskIds)
                WaitTaskState(betaTaskId, TaskState.Finished);
        }

        private ISerializer serializer;
        private IRemoteTaskQueue remoteTaskQueue;
        private TaskDataStorage taskDataStorage;
        private TaskMetaStorage taskMetaStorage;

        protected class Creater
        {
            public Creater(string taskName, long delay, Func<ITaskData> create)
            {
                TaskName = taskName;
                Delay = new TimeSpan(delay * 10000000);
                Create = create;
            }

            public Func<ITaskData> Create { get; private set; }
            public string TaskName { get; private set; }
            public TimeSpan Delay { get; private set; }
        }

        protected class AddTaskInfo
        {
            public AddTaskInfo(List<string> ids, DateTime addTime)
            {
                ids.Reverse();
                Ids = ids;
                AddTime = addTime;
            }

            public List<string> Ids { get; private set; }
            public DateTime AddTime { get; private set; }

            public AddTaskInfo Add(AddTaskInfo other)
            {
                var resId = new List<string>();
                resId.AddRange(Ids);
                resId.AddRange(other.Ids);
                var dateTime = new DateTime(Math.Min(AddTime.Ticks, other.AddTime.Ticks));
                resId.Reverse();
                return new AddTaskInfo(resId, dateTime);
            }
        }
    }
}