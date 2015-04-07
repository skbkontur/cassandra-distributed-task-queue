using System;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Http
{
    public class ElasticMonitoringHttpHandler : IHttpHandler
    {
        public ElasticMonitoringHttpHandler(ITaskIndexController taskIndexController,
                                            TaskSearchIndexSchema taskSearchIndexSchema,
                                            TaskSearchDynamicSettings taskSearchDynamicSettings,
                                            IGlobalTime globalTime)
        {
            this.taskIndexController = taskIndexController;
            this.taskSearchIndexSchema = taskSearchIndexSchema;
            this.taskSearchDynamicSettings = taskSearchDynamicSettings;
            this.globalTime = globalTime;
        }

        [HttpMethod]
        public ElasticMonitoringStatus GetStatus()
        {
            return new ElasticMonitoringStatus()
                {
                    DistributedLockAcquired = taskIndexController.IsDistributedLockAcquired(),
                    MinTicksHack = taskIndexController.MinTicksHack
                };
        }

        [HttpMethod]
        public void UpdateAndFlush()
        {
            ThrowIfDisabled();
            //note method for tests
            taskIndexController.ProcessNewEvents();
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
            taskIndexController.SetMinTicksHack(ticks);
            //note method for tests only
            taskSearchIndexSchema.DeleteAll();
        }

        private readonly ITaskIndexController taskIndexController;
        private readonly TaskSearchIndexSchema taskSearchIndexSchema;
        private readonly TaskSearchDynamicSettings taskSearchDynamicSettings;
        private readonly IGlobalTime globalTime;
    }
}