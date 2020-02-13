using GroboContainer.Infection;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas;
using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    [IgnoredImplementation]
    public class TestTaskDataRegistry : RtqTaskDataRegistryBase
    {
        public TestTaskDataRegistry()
        {
            Register<FakeFailTaskData>();
            Register<FakePeriodicTaskData>();
            Register<FakeMixedPeriodicAndFailTaskData>();
            Register<SimpleTaskData>();

            Register<SlowTaskData>();
            Register<AlphaTaskData>();
            Register<BetaTaskData>();
            Register<GammaTaskData>();
            Register<DeltaTaskData>();
            Register<EpsilonTaskData>();
            Register<FailingTaskData>();

            Register<ChainTaskData>();

            Register<TimeGuidTaskData>();
        }
    }
}