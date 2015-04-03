using System;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.TaskSearch;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Http
{
    public class ElasticMonitoringHttpHandler : IHttpHandler
    {
        public ElasticMonitoringHttpHandler(TaskSearchConsumer taskSearchConsumer,
                                            CurrentMetaProvider currentMetaProvider,
                                            TaskSearchIndexSchema taskSearchIndexSchema,
                                            TaskSearchDynamicSettings taskSearchDynamicSettings,
                                            IGlobalTime globalTime)
        {
            this.taskSearchConsumer = taskSearchConsumer;
            this.currentMetaProvider = currentMetaProvider;
            this.taskSearchIndexSchema = taskSearchIndexSchema;
            this.taskSearchDynamicSettings = taskSearchDynamicSettings;
            this.globalTime = globalTime;
        }

        [HttpMethod]
        public ElasticMonitoringStatus GetStatus()
        {
            return new ElasticMonitoringStatus()
                {
                    IsProcessingQueue = taskSearchConsumer.IsWorking(),
                    DistributedLockAcquired = taskSearchConsumer.IsDistributedLockAcquired(),
                    MinTicksHack = taskSearchConsumer.MinTicksHack
                };
        }

        [HttpMethod]
        public void UpdateAndFlush()
        {
            ThrowIfDisabled();
            if(!taskSearchConsumer.IsWorking())
                throw new Exception("Not working");
            //note method for tests
            currentMetaProvider.FetchMetas();
            taskSearchConsumer.ProcessQueue();
            taskSearchIndexSchema.Refresh();
        }

        private void ThrowIfDisabled()
        {
            if(!taskSearchDynamicSettings.EnableDestructiveActions)
                throw new InvalidOperationException("Destructive actions disabled");
        }

        [HttpMethod]
        public void DeleteAll()
        {
            ThrowIfDisabled();
            var ticks = globalTime.GetNowTicks();
            taskSearchConsumer.SetMinTicksHack(ticks);
            //note method for tests only
            taskSearchIndexSchema.DeleteAll();
        }

        private readonly TaskSearchConsumer taskSearchConsumer;
        private readonly CurrentMetaProvider currentMetaProvider;
        private readonly TaskSearchIndexSchema taskSearchIndexSchema;
        private readonly TaskSearchDynamicSettings taskSearchDynamicSettings;
        private readonly IGlobalTime globalTime;
    }
}