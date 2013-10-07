using GroboTrace;

using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace ExchangeService
{
    internal class EntryPoint : ApplicationBase
    {
        protected override void ConfigureTracingWrapper(TracingWrapperConfigurator configurator)
        {
        }

        protected override string ConfigFileName { get { return "exchangeServiceSettings"; } }

        private static void Main()
        {
            new EntryPoint().Run();
        }

        private void Run()
        {
            Container.ConfigureLockRepository();
            Container.Get<HttpService>().Run();
        }
    }
}