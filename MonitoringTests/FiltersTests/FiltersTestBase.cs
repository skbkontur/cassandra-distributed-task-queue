using System;
using System.Collections.Generic;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class FiltersTestBase : MonitoringFunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            remoteTaskQueue = container.Get<IRemoteTaskQueue>();
        }

        protected class Creater
        {
            public Creater(string taskName, int delay, Func<ITaskData> create)
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
                for(int i = 0; i < iteration; i++)
                    ids.Add(remoteTaskQueue.Queue(creater.Create(), creater.Delay));
                result.Add(creater.TaskName, new AddTaskInfo(ids, addTime));
                System.Threading.Thread.Sleep(1000);
            }
            return result;
        }

        protected void DoCheck(TasksListPage tasksListPage, AddTaskInfo addTaskInfo)
        {
            addTaskInfo.Ids.Reverse();
            tasksListPage.CheckTaskListItemsCount(addTaskInfo.Ids.Count);
            for (int i = 0; i < addTaskInfo.Ids.Count; i++)
            {
                var task = tasksListPage.GetTaskListItem(i);
                task.TaskId.WaitText(addTaskInfo.Ids[i]);
                task.TaskState.WaitText("Finished");
            }
            addTaskInfo.Ids.Reverse();
        }

        private IRemoteTaskQueue remoteTaskQueue;
    }
}