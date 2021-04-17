using System;
using System.Threading;

using GroboContainer.Core;
using GroboContainer.Impl;

using GroBuf;
using GroBuf.DataMembersExtracters;

using SkbKontur.Graphite.Client;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common
{
    public static class ApplicationBase
    {
        public static IContainer Initialize()
        {
            LoggingConfigurationHelpers.SetupUnhandledExceptionLogging();
            LoggingConfigurationHelpers.Init();

            SetUpThreadPool();

            var container = new Container(new ContainerConfiguration(AssembliesLoader.Load(), "root", ContainerMode.UseShortLog));
            container.Configurator.ForAbstraction<ILog>().UseInstances(Log.DefaultLogger);
            container.Configurator.ForAbstraction<IGraphiteClient>().UseType<NoOpGraphiteClient>();
            container.Configurator.ForAbstraction<ISerializer>().UseInstances(new Serializer(new AllPropertiesExtractor()));
            return container;
        }

        private static void SetUpThreadPool(int multiplier = 32)
        {
            if (multiplier <= 0)
                throw new InvalidOperationException($"Unable to setup minimum threads with multiplier: {multiplier}");

            var minimumThreads = Math.Min(Environment.ProcessorCount * multiplier, maximumThreads);
            ThreadPool.SetMaxThreads(maximumThreads, maximumThreads);
            ThreadPool.SetMinThreads(minimumThreads, minimumThreads);
        }

        private const int maximumThreads = 32767;
    }
}