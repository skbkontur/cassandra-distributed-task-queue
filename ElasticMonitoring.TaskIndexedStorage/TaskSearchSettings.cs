using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage
{
    public class TaskSearchSettings
    {
        public const int BulkBatchSize = 200;
        public const string SearchRequestExpirationTime = "10m";
        public const int SearchPageSize = 20;
        public static readonly TimeSpan IndexInterval = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan SyncLoadInterval = TimeSpan.FromHours(1);
    }
}