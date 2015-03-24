using System;
using System.Collections.Generic;
using System.Threading;

using GroboContainer.Infection;

using log4net;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public class CurrentMetaProvider : ICurrentMetaProvider
    {
        [ContainerConstructor]
        public CurrentMetaProvider(IEventLogRepository eventLogRepository,
                                   IGlobalTime globalTime,
                                   IHandleTasksMetaStorage handleTasksMetaStorage)
            : this(MetaProviderSettings.EventGarbageCollectionTimeout.Ticks,
                   MetaProviderSettings.MaxBatch,
                   eventLogRepository, globalTime, handleTasksMetaStorage)
        {
        }

        public CurrentMetaProvider(long maxEventLifetimeTicks,
                                   int maxBatch,
                                   IEventLogRepository eventLogRepository,
                                   IGlobalTime globalTime,
                                   IHandleTasksMetaStorage handleTasksMetaStorage)
        {
            this.globalTime = globalTime;
            loggerName = GetType().Name;
            impl = new Lazy<MetaProviderImpl>(
                () =>
                    {
                        var metaProviderImpl = new MetaProviderImpl(maxEventLifetimeTicks,
                                                                    maxBatch,
                                                                    globalTime.GetNowTicks(),
                                                                    eventLogRepository,
                                                                    handleTasksMetaStorage,
                                                                    loggerName);
                        return metaProviderImpl;
                    }, true);
            Start();
        }

        public void FetchMetas()
        {
            if(!CanWork())
            {
                logger.LogInfoFormat(loggerName, "Fetch request - stopped");
                return;
            }
            var queuedMetaConsumers = GetList();
            impl.Value.LoadMetas(globalTime.GetNowTicks(), queuedMetaConsumers);
        }

        public MetaProviderSnapshot GetSnapshotOrNull(int maxLength)
        {
            return impl.Value.GetSnapshotOrNull(maxLength);
        }

        public void Start()
        {
            logger.LogInfoFormat(loggerName, "Start");
            Interlocked.Increment(ref workingIfGreaterThanZero);
        }

        public void Stop()
        {
            var value = Interlocked.Decrement(ref workingIfGreaterThanZero);
            logger.LogInfoFormat(loggerName, "MetaProvider Stop. stopped = {0}", value <= 0);
        }

        private bool CanWork()
        {
            return Interlocked.CompareExchange(ref workingIfGreaterThanZero, 0, 0) > 0;
        }

        private IMetaConsumer[] GetList()
        {
            lock(consumersLock)
                return consumers.ToArray();
        }

        public long NowTicks { get { return globalTime.GetNowTicks(); } }

        public void Subscribe(IMetaConsumer c)
        {
            lock(consumersLock)
            {
                if(consumers.FindIndex(consumer => ReferenceEquals(consumer, c)) < 0)
                    consumers.Add(c);
            }
        }

        public void Unsubscribe(IMetaConsumer c)
        {
            lock(consumersLock)
            {
                consumers.RemoveAll(consumer => ReferenceEquals(consumer, c));
            }
        }

        private readonly string loggerName;

        private readonly Lazy<MetaProviderImpl> impl;

        private readonly IGlobalTime globalTime;

        private int workingIfGreaterThanZero;

        private static readonly ILog logger = LogManager.GetLogger("CurrentMetaProvider");

        private readonly object consumersLock = new object();
        private readonly List<IMetaConsumer> consumers = new List<IMetaConsumer>();
    }
}