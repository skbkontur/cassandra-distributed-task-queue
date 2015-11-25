using RemoteQueue.Configuration;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace ExchangeService.Http
{
    public class ExchangeServiceHttpHandler : IHttpHandler
    {
        public ExchangeServiceHttpHandler(IExchangeSchedulableRunner exchangeSchedulableRunner)
        {
            runner = exchangeSchedulableRunner;
        }

        [HttpMethod]
        public void Start()
        {
            runner.Start();
        }

        [HttpMethod]
        public void Stop()
        {
            runner.Stop();
        }

        private readonly IExchangeSchedulableRunner runner;
    }
}