using System;

using GroboContainer.Core;

using RemoteQueue.Configuration;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;
using SKBKontur.Catalogue.ServiceLib.Settings;

namespace ExchangeService.Http
{
    public class ExchangeServiceHttpHandler : IHttpHandler
    {
        public ExchangeServiceHttpHandler(IApplicationSettings applicationSettings, IContainer container, IExchangeSchedulableRunner exchangeSchedulableRunner)
        {
            runner = exchangeSchedulableRunner;
        }

        [HttpMethod]
        public void Start()
        {
            Console.WriteLine("Start");
            runner.Start();
        }

        [HttpMethod]
        public void Stop()
        {
            Console.WriteLine("Stop");
            runner.Stop();
        }

        private readonly IExchangeSchedulableRunner runner;
    }
}