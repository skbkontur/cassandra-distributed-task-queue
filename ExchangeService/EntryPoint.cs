using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Configuration;
using RemoteQueue.Handling;
using RemoteQueue.Settings;
using RemoteQueue.UserClasses;

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
            Container.Get<HttpService>().Run();
        }
    }
}