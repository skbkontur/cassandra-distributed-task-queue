using System;
using System.Text;

using JetBrains.Annotations;

using Newtonsoft.Json;

using RemoteTaskQueue.FunctionalTests.Common;

using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    public abstract class HttpClientForTestsBase
    {
        protected HttpClientForTestsBase(int devPort, TimeSpan defaultRequestTimeout)
        {
            var clusterClientLogger = Log.For("HttpClientForTests").WithMinimumLevel(LogLevel.Warn);
            clusterClient = new ClusterClient(clusterClientLogger,
                                              configuration =>
                                                  {
                                                      configuration.SetupUniversalTransport(new UniversalTransportSettings
                                                          {
                                                              AllowAutoRedirect = false,
                                                              TcpKeepAliveEnabled = true,
                                                              MaxConnectionsPerEndpoint = 4096,
                                                          });
                                                      configuration.DefaultConnectionTimeout = TimeSpan.FromMilliseconds(750);
                                                      configuration.DefaultTimeout = defaultRequestTimeout;

                                                      configuration.ClusterProvider = new FixedClusterProvider(new Uri($"http://localhost:{devPort}"));
                                                      configuration.DefaultRequestStrategy = Strategy.Sequential1;

                                                      configuration.AddRequestTransform(request => request.Content == null || request.Content.Length == 0
                                                                                                       ? request.WithHeader("Content", "no")
                                                                                                       : request);
                                                  });
        }

        [NotNull]
        protected RequestResult Post([NotNull] string methodName)
        {
            return Post(methodName, requestContent : null);
        }

        [NotNull]
        protected RequestResult Post<T1>([NotNull] string methodName, [CanBeNull] T1 arg1)
        {
            return Post(methodName, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(arg1)));
        }

        [NotNull]
        private RequestResult Post([NotNull] string methodName, [CanBeNull] byte[] requestContent)
        {
            var request = Request.Post(methodName);
            if (requestContent != null)
                request = request.WithContent(requestContent).WithHeader("Content-Type", "application/json");

            using (var result = clusterClient.SendAsync(request).GetAwaiter().GetResult())
            {
                if (result.Status != ClusterResultStatus.Success || result.Response.Code != ResponseCode.Ok)
                    throw new InvalidOperationException($"Request failed: Request: {result.Request}\nStatus: {result.Status}\nSelected response: {result.Response}");
                return new RequestResult(result.Response.Content.ToArray());
            }
        }

        private readonly IClusterClient clusterClient;
    }

    public class RequestResult
    {
        public RequestResult([NotNull] byte[] serializedResult)
        {
            this.serializedResult = serializedResult;
        }

        [NotNull]
        public T ThenReturn<T>()
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(serializedResult));
        }

        private readonly byte[] serializedResult;
    }
}