using ExchangeService.Settings;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;

using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace ExchangeService
{
    public class ExchangeServiceEntryPoint : ApplicationBase
    {
        protected override string ConfigFileName => "exchangeService.csf";
        protected override string SubProject => null;

        private static void Main()
        {
            new ExchangeServiceEntryPoint().Run();
        }

        private void Run()
        {
            Container.ConfigureCassandra();
            Container.ConfigureRemoteTaskQueueForConsumer<RtqConsumerSettings, RtqTaskHandlerRegistry>();
            Container.Configurator.ForAbstraction<ITestTaskLogger>().UseType<TestTaskLogger>();
            Container.Configurator.ForAbstraction<ITestCounterRepository>().UseType<TestCounterRepository>();
            Container.Configurator.ForAbstraction<IHttpHandler>().UseType<ExchangeServiceHttpHandler>();
            Container.Get<HttpService>().Run();
        }
    }
}