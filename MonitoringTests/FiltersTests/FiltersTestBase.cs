using System;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.FiltersTests
{
    public class FiltersTestBase : MonitoringFunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp(); 
            remoteTaskQueue = container.Get<IRemoteTaskQueue>();
            AddTasks();
        }

        private void AddTasks()
        {
            const int cnt = 7;
            for (int i = 0; i < cnt; i++)
                remoteTaskQueue.Queue(new AlphaTaskData(), TimeSpan.FromSeconds(11));
            System.Threading.Thread.Sleep(1000);
            for (int i = 0; i < cnt; i++)
                remoteTaskQueue.Queue(new BetaTaskData(), TimeSpan.FromSeconds(7));
            System.Threading.Thread.Sleep(1000);
            for (int i = 0; i < cnt; i++)
                remoteTaskQueue.Queue(new DeltaTaskData(), TimeSpan.FromSeconds(3));
        }

        private IRemoteTaskQueue remoteTaskQueue;
    }
}