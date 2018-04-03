using System;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public static class CounterSettings
    {
        public static readonly TimeSpan NewEventsWatchInterval = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan EventGarbageCollectionTimeout = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan MaxHistoryDepth = TimeSpan.FromDays(3);
        public static readonly TimeSpan CounterUpdateInterval = TimeSpan.FromSeconds(2);
        public static readonly TimeSpan CounterSaveSnapshotInterval = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan SlowCalculationInterval = TimeSpan.FromSeconds(10);
        public const int MaxStoredSnapshotLength = 200000;
        public const int MaxBatch = 1000;
        public static readonly TimeSpan GraphitePostInterval = TimeSpan.FromMinutes(1);
    }
}