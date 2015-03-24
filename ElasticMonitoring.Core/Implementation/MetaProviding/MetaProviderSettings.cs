using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public class MetaProviderSettings
    {
        public const int MaxBatch = 1000;
        public static readonly TimeSpan FetchMetasInterval = TimeSpan.FromSeconds(2);
        public static readonly TimeSpan EventGarbageCollectionTimeout = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan MaxHistoryDepth = TimeSpan.FromDays(3);
        public static readonly TimeSpan SlowCalculationIntervalMs = TimeSpan.FromSeconds(10);
    }
}