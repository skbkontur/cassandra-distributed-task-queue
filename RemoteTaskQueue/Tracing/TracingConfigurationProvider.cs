using System;

using Kontur.Tracing;

namespace RemoteQueue.Tracing
{
    internal class TracingConfigurationProvider : IConfigurationProvider
    {
        public TracingConfig GetConfig()
        {
            return new TracingConfig
                {
                    IsEnabled = true,
                    AggregationServiceSystem = "edi-test",
                    AggregationServiceURL = "http://vm-elastic:9003/spans",
                    BufferFlushPeriod = TimeSpan.FromSeconds(5),
                    BufferFlushTimeout = TimeSpan.FromSeconds(30),
                    MaxBufferedSpans = 1000,
                    MaxSamplesPerSecond = 10,
                    SamplingChance = 1d
                };
        }
    }
}
