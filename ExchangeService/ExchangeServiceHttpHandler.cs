using System;

using RemoteQueue.Configuration;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace ExchangeService
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

        [HttpMethod]
        public void ChangeTaskTtl(TimeSpan ttl)
        {
#pragma warning disable 618
            runner.RemoteTaskQueueBackdoor.ChangeTaskTtl(ttl);
#pragma warning restore 618
        }

        private readonly IExchangeSchedulableRunner runner;
    }
}