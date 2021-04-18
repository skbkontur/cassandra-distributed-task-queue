using GroboContainer.Core;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses;
using SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService
{
    public class RtqTaskHandlerRegistry : RtqTaskHandlerRegistryBase
    {
        public RtqTaskHandlerRegistry(IContainer container, IRtqTaskDataRegistry taskDataRegistry)
            : base(taskDataRegistry)
        {
            this.container = container;
            Register<FakeFailTaskHandler>();
            Register<FakePeriodicTaskHandler>();
            Register<FakeMixedPeriodicAndFailTaskHandler>();
            Register<SimpleTaskHandler>();
            Register<AlphaTaskHandler>();
            Register<BetaTaskHandler>();
            Register<GammaTaskHandler>();
            Register<DeltaTaskHandler>();
            Register<EpsilonTaskHandler>();
            Register<SlowTaskHandler>();
            Register<ChainTaskHandler>();
            Register<FailingTaskHandler>();
            Register<TimeGuidTaskHandler>();
        }

        private void Register<THandler>()
            where THandler : IRtqTaskHandler
        {
            Register(() => container.Create<THandler>());
        }

        private readonly IContainer container;
    }
}