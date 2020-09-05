using System.IO;
using System.Linq;
using System.Reflection;

using GroboContainer.Core;
using GroboContainer.Impl;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;
using SkbKontur.Cassandra.DistributedTaskQueue.TestApi.Controllers;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestApi
{
    public class GroboControllerFactory : IControllerFactory
    {
        public GroboControllerFactory()
        {
            var entryAssembly = Assembly.GetEntryAssembly().Location;
            var assemblies = Directory.EnumerateFiles(Path.GetDirectoryName(entryAssembly), "*.dll", SearchOption.TopDirectoryOnly)
                                      .Where(x => Path.GetFileName(x).StartsWith("SkbKontur.Cassandra.DistributedTaskQueue"))
                                      .Select(Assembly.LoadFrom);
            groboContainer = new Container(new ContainerConfiguration(assemblies));
            groboContainer.Configurator.ForAbstraction<IRtqMonitoringApi>().UseType<RtqMonitoringApiStub>();
        }

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

        private readonly IContainer groboContainer;
    }
}