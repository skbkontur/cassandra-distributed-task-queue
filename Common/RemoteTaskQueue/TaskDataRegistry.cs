using RemoteQueue.Configuration;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue
{
    public class TaskDataRegistry : TaskDataRegistryBase
    {
        public TaskDataRegistry()
        {
            Register<FakeFailTaskData>();
            Register<FakePeriodicTaskData>();
            Register<SimpleTaskData>();
            Register<ByteArrayTaskData>();
            Register<ByteArrayAndNestedTaskData>();
            Register<FileIdTaskData>();

            Register<SlowTaskData>();
            Register<AlphaTaskData>();
            Register<BetaTaskData>();
            Register<DeltaTaskData>();
            Register<FailingTaskData>();

            Register<ChainTaskData>();
        }
    }
}