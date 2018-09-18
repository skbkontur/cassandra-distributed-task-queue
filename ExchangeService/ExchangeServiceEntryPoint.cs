using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace ExchangeService
{
    public class ExchangeServiceEntryPoint : ApplicationBase
    {
        protected override string ConfigFileName => "exchangeService.csf";

        private static void Main()
        {
            new ExchangeServiceEntryPoint().Run();
        }

        private void Run()
        {
            Container.ConfigureForTestRemoteTaskQueue();
            Container.Get<HttpService>().Run();
        }
    }
}