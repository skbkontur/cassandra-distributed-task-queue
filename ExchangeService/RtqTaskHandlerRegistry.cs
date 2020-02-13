using ExchangeService.UserClasses;
using ExchangeService.UserClasses.MonitoringTestTaskData;

using GroboContainer.Core;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService
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