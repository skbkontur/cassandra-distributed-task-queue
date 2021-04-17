using System;

namespace RemoteTaskQueue.FunctionalTests
{
    public class ExchangeServiceReplicaClient : HttpClientForTestsBase
    {
        public ExchangeServiceReplicaClient(int port)
            : base(port, defaultRequestTimeout : TimeSpan.FromSeconds(30))
        {
        }

        public void Start()
        {
            Post("Start");
        }

        public void Stop()
        {
            Post("Stop");
        }

        public void ChangeTaskTtl(TimeSpan ttl)
        {
            Post("ChangeTaskTtl", ttl);
        }
    }
}