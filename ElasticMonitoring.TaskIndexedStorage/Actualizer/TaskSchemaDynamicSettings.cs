using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer
{
    public class TaskSchemaDynamicSettings
    {
        public TaskSchemaDynamicSettings(IApplicationSettings applicationSettings)
        {
            if(!applicationSettings.TryGetInt(prefixSettings + "NumberOfShards", out numberOfShards))
                numberOfShards = 1;
            if(!applicationSettings.TryGetInt(prefixSettings + "NumberOfReplicas", out replicaCount))
                replicaCount = 1;

            IndexPrefix = applicationSettings.GetString(prefixSettings + "IndexPrefix");
            LastTicksIndex = applicationSettings.GetString(prefixSettings + "LastTicksIndex");
            OldDataIndex = applicationSettings.GetString(prefixSettings + "OldDataIndex");
            SearchAliasFormat = applicationSettings.GetString(prefixSettings + "SearchAliasFormat");
            OldDataAliasFormat = applicationSettings.GetString(prefixSettings + "OldDataAliasFormat");
            TemplateNamePrefix = applicationSettings.GetString(prefixSettings + "TemplateNamePrefix");
        }

        private const string prefixSettings = "ElasticSearchSchema.MonitoringSearch.";

        public string IndexPrefix { get; private set; }
        public string LastTicksIndex { get; private set; }
        public string OldDataIndex { get; private set; }
        public string SearchAliasFormat { get; private set; }
        public string OldDataAliasFormat { get; private set; }
        public string TemplateNamePrefix { get; private set; }

        public int NumberOfShards { get { return numberOfShards; } }
        public int ReplicaCount { get { return replicaCount; } }

        private readonly int numberOfShards;
        private readonly int replicaCount;
    }
}