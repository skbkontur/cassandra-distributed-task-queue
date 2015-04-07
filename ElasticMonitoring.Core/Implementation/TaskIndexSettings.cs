using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskIndexSettings
    {
        public static readonly TimeSpan CacheInterval = TimeSpan.FromMinutes(10);
        public static readonly int MaxBatch = 200;
    }
}