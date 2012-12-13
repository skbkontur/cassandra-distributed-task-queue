using SKBKontur.Catalogue.RemoteTaskQueue.Common;

namespace FunctionalTests
{
    public class FunctionalTestBase : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            var exchangeServiceClient = Container.Get<IExchangeServiceClient>();
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