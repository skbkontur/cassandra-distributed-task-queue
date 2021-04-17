using GroboContainer.Core;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;

namespace ExchangeService
{
    public class GroboControllerFactory : IControllerFactory
    {
        public object CreateController(ControllerContext controllerContext)
        {
            var controllerType = controllerContext.ActionDescriptor.ControllerTypeInfo.AsType();
            var controller = groboContainer.Create(controllerType);
            ((ControllerBase)controller).ControllerContext = controllerContext;
            return controller;
        }

        public void ReleaseController(ControllerContext context, object controller)
        {
        }

        private static IContainer ConfigureContainer()
        {
            var container = ApplicationBase.Initialize();
            container.ConfigureCassandra();
            container.ConfigureRemoteTaskQueueForConsumer<RtqConsumerSettings, RtqTaskHandlerRegistry>();
            container.Configurator.ForAbstraction<ITestTaskLogger>().UseType<TestTaskLogger>();
            container.Configurator.ForAbstraction<ITestCounterRepository>().UseType<TestCounterRepository>();
            return container;
        }

        private static readonly IContainer groboContainer = ConfigureContainer();
    }
}