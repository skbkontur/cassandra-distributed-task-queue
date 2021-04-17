using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService
{
    public class RtqConsumerSettings : IRtqConsumerSettings
    {
        public TimeSpan PeriodicInterval => TimeSpan.FromMilliseconds(100);
        public int MaxRunningTasksCount => 20;
        public int MaxRunningContinuationsCount => 20;
    }
}