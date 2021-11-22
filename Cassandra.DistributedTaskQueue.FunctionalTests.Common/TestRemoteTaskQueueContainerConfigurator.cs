using System;
using System.Linq;
using System.Net;

using GroboContainer.Core;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common
{
    public static class TestRemoteTaskQueueContainerConfigurator
    {
        public static void ConfigureCassandra([NotNull] this IContainer container)
        {
            var cassandraAddress = Environment.GetEnvironmentVariable("CASSANDRA_ADDRESS") ?? "127.0.0.1";
            var localEndPoint = new IPEndPoint(GetIpV4Address(cassandraAddress), 9160);
            var cassandraClusterSettings = new CassandraClusterSettings
                {
                    Endpoints = new[] {localEndPoint},
                    EndpointForFierceCommands = localEndPoint,
                    ReadConsistencyLevel = ConsistencyLevel.QUORUM,
                    WriteConsistencyLevel = ConsistencyLevel.QUORUM,
                    Attempts = 5,
                    Timeout = 6000,
                    FierceTimeout = 6000,
                    ConnectionIdleTimeout = TimeSpan.FromMinutes(10),
                };
            var cassandraCluster = new CassandraCluster(cassandraClusterSettings, container.Get<ILog>());
            container.Configurator.ForAbstraction<ICassandraCluster>().UseInstances(cassandraCluster);
        }

        [NotNull]
        public static IContainer ConfigureRemoteTaskQueueForConsumer<TRtqConsumerSettings, TTaskHandlerRegistry>([NotNull] this IContainer container)
            where TRtqConsumerSettings : IRtqConsumerSettings
            where TTaskHandlerRegistry : IRtqTaskHandlerRegistry
        {
            return container.ConfigureRemoteTaskQueueForConsumer<TRtqConsumerSettings>(() => container.Get<TTaskHandlerRegistry>());
        }

        [NotNull]
        private static IContainer ConfigureRemoteTaskQueueForConsumer<TRtqConsumerSettings>([NotNull] this IContainer container, [NotNull] Func<IRtqTaskHandlerRegistry> getTaskHandlerRegistry)
            where TRtqConsumerSettings : IRtqConsumerSettings
        {
            container.ConfigureRemoteTaskQueue(out var remoteTaskQueue);
            container.Configurator.ForAbstraction<IRtqConsumerSettings>().UseType<TRtqConsumerSettings>();
            container.Configurator.ForAbstraction<IRtqTaskHandlerRegistry>().UseInstances(getTaskHandlerRegistry());
            var rtqConsumer = container.Create<RemoteTaskQueue, RtqConsumer>(remoteTaskQueue);
            container.Configurator.ForAbstraction<IRtqConsumer>().UseInstances(rtqConsumer);
            return container;
        }

        [NotNull]
        public static IContainer ConfigureRemoteTaskQueue([NotNull] this IContainer container, out RemoteTaskQueue remoteTaskQueue)
        {
            container.Configurator.ForAbstraction<IRtqSettings>().UseType<TestRtqSettings>();
            container.Configurator.ForAbstraction<IGlobalTime>().UseType<GlobalTimeProxy>();
            container.Configurator.ForAbstraction<IRtqPeriodicJobRunner>().UseType<TestRtqPeriodicJobRunner>();

            container.Configurator.ForAbstraction<IRtqTaskDataRegistry>().UseInstances(new TestTaskDataRegistry());
            remoteTaskQueue = container.Create<IRtqProfiler, RemoteTaskQueue>(new NoOpRtqProfiler());
            container.Configurator.ForAbstraction<IRtqTaskProducer>().UseInstances(remoteTaskQueue);
            container.Configurator.ForAbstraction<IRtqTaskManager>().UseInstances(remoteTaskQueue);
            return container;
        }
        
        private static IPAddress GetIpV4Address([NotNull] string hostNameOrIpAddress)
        {
            if (IPAddress.TryParse(hostNameOrIpAddress, out var res))
                return res;

            return Dns.GetHostEntry(hostNameOrIpAddress).AddressList.First(address => !address.ToString().Contains(':'));
        }
    }
}