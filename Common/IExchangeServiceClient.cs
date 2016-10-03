using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common
{
    public interface IExchangeServiceClient
    {
        void Start();
        void Stop();
        void ChangeTaskTtl(TimeSpan ttl);
    }
}