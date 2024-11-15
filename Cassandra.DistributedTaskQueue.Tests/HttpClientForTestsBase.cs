using System;
using System.Text;
using System.Text.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;

using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests;

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

    protected RequestResult Post(string methodName)
    {
        return Post(methodName, requestContent : null);
    }

    protected RequestResult Post<T1>(string methodName, T1 arg1)
    {
        return Post(methodName, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(arg1)));
    }

    private RequestResult Post(string methodName, byte[] requestContent)
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
    public RequestResult(byte[] serializedResult)
    {
        this.serializedResult = serializedResult;
    }

    public T ThenReturn<T>()
    {
        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(serializedResult));
    }

    private readonly byte[] serializedResult;
}