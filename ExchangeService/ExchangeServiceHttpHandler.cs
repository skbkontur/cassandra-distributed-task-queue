using System;
using System.Reflection;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace ExchangeService
{
    public class ExchangeServiceHttpHandler : IHttpHandler
    {
        public ExchangeServiceHttpHandler(IRtqConsumer rtqConsumer)
        {
            this.rtqConsumer = (RtqConsumer)rtqConsumer;
        }

        [HttpMethod]
        public void Start()
        {
            rtqConsumer.Start();
        }

        [HttpMethod]
        public void Stop()
        {
            rtqConsumer.Stop();
        }

        [HttpMethod]
        public void ChangeTaskTtl(TimeSpan ttl)
        {
            var internals = rtqInternals.GetValue(rtqConsumer);
            changeTtlMethod.Invoke(internals, new object[] {ttl});
        }

        private readonly RtqConsumer rtqConsumer;

        private static readonly PropertyInfo rtqInternals = typeof(RtqConsumer).GetProperty("RtqInternals", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo changeTtlMethod = rtqInternals.PropertyType.GetMethod("ChangeTaskTtl");
    }
}