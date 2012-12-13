using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace ExchangeService
{
    internal class EntryPoint : ApplicationBase
    {
        protected override string ConfigFileName { get { return "exchangeServiceSettings"; } }

        private static void Main()
        {
            new EntryPoint().Run();
        }

        private void Run()
        {
            //Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<CassandraSettings>());
            Container.Get<HttpService>().Run();
        }
    }
}