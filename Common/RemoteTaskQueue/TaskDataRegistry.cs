using RemoteQueue.UserClasses;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue
{
    public class TaskDataRegistry : TaskDataRegistryBase
    {
        public TaskDataRegistry()
        {
            Register<FakeFailTaskData>("FakeFailTaskData");
            Register<FakePeriodicTaskData>("FakePeriodicTaskData");
            Register<SimpleTaskData>("SimpleTaskData");
            Register<ByteArrayTaskData>("ByteArrayTaskData");
            Register<FileIdTaskData>("FileIdTaskData");

            Register<SlowTaskData>("SlowTaskData");
            Register<AlphaTaskData>("AlphaTaskData");
            Register<BetaTaskData>("BetaTaskData");
            Register<DeltaTaskData>("DeltaTaskData");
        }
    }
}