using System;

using GroboContainer.Core;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedLock.RemoteLocker;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.GlobalTimestamp;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    public static class TestRemoteTaskQueueContainerConfigurator
    {
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
            var rtqConsumer = container.Create<SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue, RtqConsumer>(remoteTaskQueue);
            container.Configurator.ForAbstraction<IRtqConsumer>().UseInstances(rtqConsumer);
            return container;
        }

        [NotNull]
        public static IContainer ConfigureRemoteTaskQueue([NotNull] this IContainer container, out SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue remoteTaskQueue)
        {
            ConfigureRemoteLock(container);
            container.Configurator.ForAbstraction<IRtqSettings>().UseType<TestRtqSettings>();
            container.Configurator.ForAbstraction<IGlobalTime>().UseType<GlobalTimeProxy>();
            container.Configurator.ForAbstraction<IRtqPeriodicJobRunner>().UseType<TestRtqPeriodicJobRunner>();

            container.Configurator.ForAbstraction<IRtqTaskDataRegistry>().UseInstances(new TestTaskDataRegistry());
            remoteTaskQueue = container.Create<IRtqProfiler, SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue>(new NoOpRtqProfiler());
            container.Configurator.ForAbstraction<IRtqTaskProducer>().UseInstances(remoteTaskQueue);
            container.Configurator.ForAbstraction<IRtqTaskManager>().UseInstances(remoteTaskQueue);
            return container;
        }

        private static void ConfigureRemoteLock(IContainer container)
        {
            var logger = container.Get<ILog>();
            const string locksKeyspace = TestRtqSettings.QueueKeyspaceName;
            const string locksColumnFamily = RtqColumnFamilyRegistry.LocksColumnFamilyName;
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(locksKeyspace, locksColumnFamily);
            var remoteLockImplementation = container.Create<CassandraRemoteLockImplementationSettings, CassandraRemoteLockImplementation>(remoteLockImplementationSettings);
            var remoteLockCreator = new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics($"{locksKeyspace}_{locksColumnFamily}"), logger);
            container.Configurator.ForAbstraction<IRemoteLockCreator>().UseInstances(remoteLockCreator);
        }
    }
}