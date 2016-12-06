using System;

using log4net;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class LazySchemaActualizer
    {
        public LazySchemaActualizer(IApplicationSettings applicationSettings, Func<TaskSearchIndexSchema> getTaskSearchIndexSchema)
        {
            this.applicationSettings = applicationSettings;
            this.getTaskSearchIndexSchema = getTaskSearchIndexSchema;
        }

        public void ActualizeSchema()
        {
            bool value;
            if(applicationSettings.TryGetBool("ActualizeElasticSchemaOnStart", out value) && value)
            {
                logger.LogInfoFormat("ActualizeElasticSchemaOnStart is true");
                getTaskSearchIndexSchema().ActualizeTemplate(local : true);
            }
        }

        private readonly IApplicationSettings applicationSettings;
        private readonly Func<TaskSearchIndexSchema> getTaskSearchIndexSchema;

        private readonly ILog logger = LogManager.GetLogger("LazySchemaActualizer");
    }
}