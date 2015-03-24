using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer
{
    public class TaskSearchDynamicSettings
    {
        public TaskSearchDynamicSettings(IApplicationSettings applicationSettings)
        {
            if(!applicationSettings.TryGetInt("ElasticSearchSchema.MonitoringSearch.NumberOfShards", out numberOfShards))
                numberOfShards = 1;
            if(!applicationSettings.TryGetInt("ElasticSearchSchema.MonitoringSearch.NumberOfReplicas", out replicaCount))
                replicaCount = 1;
        }

        public int NumberOfShards { get { return numberOfShards; } }
        public int ReplicaCount { get { return replicaCount; } }
        private readonly int numberOfShards;
        private readonly int replicaCount;
    }
}