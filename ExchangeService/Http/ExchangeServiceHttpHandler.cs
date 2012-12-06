using System;

using ExchangeService.Settings;

using GroboContainer.Core;

using RemoteQueue.Configuration;
using RemoteQueue.UserClasses;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;
using SKBKontur.Catalogue.ServiceLib.Settings;

namespace ExchangeService.Http
{
    public class ExchangeServiceHttpHandler : IHttpHandler
    {
        public ExchangeServiceHttpHandler(IApplicationSettings applicationSettings, IContainer container)
        {
            runner = new ExchangeSchedulableRunner(new CassandraSettings(), new ExchangeSchedulableRunnerSettings(applicationSettings), container.Get<ITaskDataRegistry>(), container.Get<ITaskHandlerRegistry>());
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