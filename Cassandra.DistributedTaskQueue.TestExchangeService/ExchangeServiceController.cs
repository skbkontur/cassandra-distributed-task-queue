using System;

using Microsoft.AspNetCore.Mvc;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService
{
    [Route("/[action]")]
    public class ExchangeServiceController : ControllerBase
    {
        public ExchangeServiceController(IRtqConsumer rtqConsumer)
        {
            this.rtqConsumer = (RtqConsumer)rtqConsumer;
        }

        [HttpPost]
        public void Start()
        {
            rtqConsumer.Start();
        }

        [HttpPost]
        public void Stop()
        {
            rtqConsumer.Stop();
        }

        [HttpPost]
        public void ChangeTaskTtl([FromBody] TimeSpan ttl)
        {
            rtqConsumer.RtqInternals.ChangeTaskTtl(ttl);
        }

        private readonly RtqConsumer rtqConsumer;
    }
}