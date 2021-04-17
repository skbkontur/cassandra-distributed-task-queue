using GroboContainer.Infection;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common
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