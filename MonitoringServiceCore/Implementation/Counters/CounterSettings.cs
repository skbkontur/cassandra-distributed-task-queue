using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public class CounterSettings
    {
        public static readonly TimeSpan EventGarbageCollectionTimeout = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan MaxHistoryDepth = TimeSpan.FromDays(3);
        public static readonly TimeSpan CounterUpdateInterval = TimeSpan.FromSeconds(2);
        public static readonly TimeSpan CounterSaveSnapshotInterval = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan SlowCalculationInterval = TimeSpan.FromSeconds(10);
        public const int MaxStoredSnapshotLength = 2000;
        public const int MaxBatch = 1000;
    }
}