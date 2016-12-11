using RemoteQueue.Configuration;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    public class TaskDataRegistry : TaskDataRegistryBase
    {
        public TaskDataRegistry()
        {
            Register<FakeFailTaskData>();
            Register<FakePeriodicTaskData>();
            Register<FakeMixedPeriodicAndFailTaskData>();
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