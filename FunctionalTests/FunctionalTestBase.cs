using ExchangeService.Http;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

namespace FunctionalTests
{
    public class FunctionalTestBase : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            var exchangeServiceClient = Container.Get<IExchangeServiceClient>();
            //Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<CassandraSettings>());
            exchangeServiceClient.Start();
        }

        public override void TearDown()
        {
            var exchangeServiceClient = Container.Get<IExchangeServiceClient>();
            exchangeServiceClient.Stop();
            base.TearDown();
        }
    }
}