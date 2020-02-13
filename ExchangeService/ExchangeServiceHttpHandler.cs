using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace ExchangeService
{
    public class ExchangeServiceHttpHandler : IHttpHandler
    {
        public ExchangeServiceHttpHandler(RtqConsumer consumer)
        {
            this.consumer = consumer;
        }

        [HttpMethod]
        public void Start()
        {
            consumer.Start();
        }

        [HttpMethod]
        public void Stop()
        {
            consumer.Stop();
        }

        [HttpMethod]
        public void ChangeTaskTtl(TimeSpan ttl)
        {
#pragma warning disable 618
            consumer.RtqBackdoor.ChangeTaskTtl(ttl);
#pragma warning restore 618
        }

        private readonly RtqConsumer consumer;
    }
}