using System;
using System.Collections.Generic;

using NUnit.Framework;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class FatTest : FiltersTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            remoteTaskQueue = container.Get<IRemoteTaskQueue>();
            createTaskData = new Func<ITaskData>[]
                {
                    () => new AlphaTaskData(),
                    () => new BetaTaskData(),
                    () => new DeltaTaskData(),
                };
        }

        private IRemoteTaskQueue remoteTaskQueue;
        private Func<ITaskData>[] createTaskData;

        [Ignore]
        [Test]
        public void Test()
        {
            var ids = new List<string>();
            for (int i = 0; i < 6000; i++)
            {
                ids.Add(remoteTaskQueue.CreateTask(createTaskData[i % 3]()).Queue());
                if(i % 5000 == 0)
                    Console.WriteLine("Complete: {0}", i);
            }
            CreateUser("user", "psw");
            var taskListPage = Login("user", "psw");
            DoCheck(ref taskListPage, new AddTaskInfo(ids, new DateTime()));
        }
    }
}