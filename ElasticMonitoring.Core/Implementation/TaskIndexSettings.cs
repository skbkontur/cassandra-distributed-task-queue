using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskIndexSettings
    {
        public static readonly TimeSpan MetaCacheInterval = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan IndexInterval = TimeSpan.FromSeconds(5);
    }
}