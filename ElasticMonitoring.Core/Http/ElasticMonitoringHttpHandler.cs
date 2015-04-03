using System;

using log4net;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.TaskSearch;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Http
{
    public class ElasticMonitoringHttpHandler : IHttpHandler
    {
        public ElasticMonitoringHttpHandler(TaskSearchConsumer taskSearchConsumer, 
                CurrentMetaProvider currentMetaProvider, 
            TaskSearchIndex taskSearchIndex,
            TaskSearchIndexSchema taskSearchIndexSchema)
        {
            this.taskSearchConsumer = taskSearchConsumer;
            this.currentMetaProvider = currentMetaProvider;
            this.taskSearchIndex = taskSearchIndex;
            this.taskSearchIndexSchema = taskSearchIndexSchema;
        }

        [HttpMethod]
        public void UpdateAndFlush()
        {
            if (!taskSearchConsumer.IsWorking())
                throw new Exception("Not working");
            //note method for tests
            currentMetaProvider.FetchMetas();
            taskSearchConsumer.ProcessQueue();
            taskSearchIndex.Refresh();
        }

        [HttpMethod]
        public void DeleteAll()
        {
            //note method for tests only
            //taskSearchConsumer.ProcessQueue();
            //currentMetaProvider.FetchMetas();
            taskSearchIndexSchema.DeleteAll();
        }

        private readonly TaskSearchConsumer taskSearchConsumer;
        private readonly CurrentMetaProvider currentMetaProvider;

        private readonly TaskSearchIndex taskSearchIndex;
        private readonly TaskSearchIndexSchema taskSearchIndexSchema;
    }
}