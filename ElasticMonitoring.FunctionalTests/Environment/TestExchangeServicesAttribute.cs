using System;

using GroboContainer.Core;

using NUnit.Framework;

using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Container;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests.Environment
{
    [ContainerInitializer(60)]
    public class TestExchangeServicesAttribute : Attribute, IContainerInitializationAction
    {
        public void BeforeTest(IContainer container, TestDetails testDetails, object fixture)
        {
            container.Get<IExchangeServiceClient>().Start();
        }

        public void AfterTest(IContainer container, TestDetails testDetails, object fixture)
        {
            container.Get<IExchangeServiceClient>().Stop();
        }
    }
}