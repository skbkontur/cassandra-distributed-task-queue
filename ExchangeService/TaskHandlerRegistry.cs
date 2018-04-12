using ExchangeService.UserClasses;
using ExchangeService.UserClasses.MonitoringTestTaskData;

using GroboContainer.Core;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace ExchangeService
{
    public class TaskHandlerRegistry : TaskHandlerRegistryBase
    {
        public TaskHandlerRegistry(IContainer container, ITaskDataRegistry taskDataRegistry)
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
            Register<SlowTaskHandler>();
            Register<ChainTaskHandler>();
            Register<FailingTaskHandler>();
        }

        private void Register<THandler>()
            where THandler : ITaskHandler
        {
            Register(() => container.Create<THandler>());
        }

        private readonly IContainer container;
    }
}