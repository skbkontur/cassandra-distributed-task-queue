using System;
using System.Collections.Generic;

using GroBuf;

using RemoteQueue.Cassandra;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Constants;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class FiltersTestBase : MonitoringFunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            remoteTaskQueue = container.Get<IRemoteTaskQueue>();
            handleTaskCollection = container.Get<IHandleTaskCollection>();
            serializer = container.Get<ISerializer>();
        }

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
            public AddTaskInfo( List<string> ids, DateTime? addTime)
            {
                ids.Reverse();
                Ids = ids;
                AddTime = addTime;
            }

            public List<string> Ids { get; private set; }
            public DateTime? AddTime { get; set; }

            public AddTaskInfo Add(AddTaskInfo other)
            {
                var resId = new List<string>();
                resId.AddRange(Ids);
                resId.AddRange(other.Ids);
                var dateTime = AddTime.HasValue && other.AddTime.HasValue ? (DateTime?)new DateTime(Math.Min(AddTime.Value.Ticks, other.AddTime.Value.Ticks)) : null;
                resId.Reverse();
                return new AddTaskInfo(resId, dateTime);
            }
        }

        protected Dictionary<string, AddTaskInfo> AddTasks(int iteration, params Creater [] creaters)
        {
            var result = new Dictionary<string, AddTaskInfo>();
            foreach (var creater in creaters)
            {
                var ids = new List<string>();
                var addTime = DateTime.UtcNow;
                for (int i = 0; i < iteration; i++)
                {
                    var id = remoteTaskQueue.Queue(creater.Create(), creater.Delay);
                    ids.Add(id);
                    if (creater.TaskName == "BetaTaskData")
                    {
                        var task = handleTaskCollection.GetTask(id);
                        var data = serializer.Deserialize<BetaTaskData>(task.Data);
                        data.OwnTaskId = id;
                        task.Data = serializer.Serialize(data);
                        handleTaskCollection.AddTask(task);
                    }
                }
                result.Add(creater.TaskName, new AddTaskInfo(ids, addTime));
                System.Threading.Thread.Sleep(1000);
            }
            return result;
        }

        protected void DoCheck(ref TasksListPage tasksListPage, AddTaskInfo addTaskInfo)
        {
            var ids = addTaskInfo.Ids.ToArray();
            var parts = new SeparateOnBatchesEnumerable<string>(ids, ControllerConstants.DefaultRecordsNumberPerPage);
            int cnt = 0;
            foreach(var pageIds in parts)
            {
                cnt++;
                tasksListPage.CheckTaskListItemsCount(pageIds.Length);
                for (int i = 0; i < pageIds.Length; i++)
                {
                    var task = tasksListPage.GetTaskListItem(i);
                    task.TaskId.WaitText(pageIds[i]);
                }
                if(ids.Length > cnt*ControllerConstants.DefaultRecordsNumberPerPage)
                    tasksListPage = tasksListPage.GoToNextPage();
            }
        }

        private IRemoteTaskQueue remoteTaskQueue;
        protected IHandleTaskCollection handleTaskCollection;
        protected ISerializer serializer;
    }
}