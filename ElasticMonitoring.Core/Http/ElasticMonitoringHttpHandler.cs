using System;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Types;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Http
{
    public class ElasticMonitoringHttpHandler : IHttpHandler
    {
        public ElasticMonitoringHttpHandler(ITaskIndexController taskIndexController,
                                            ITaskWriteDynamicSettings settings,
                                            IGlobalTime globalTime)
        {
            this.taskIndexController = taskIndexController;
            this.settings = settings;
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
        public void Update()
        {
            ThrowIfDisabled();
            //note method for tests
            taskIndexController.ProcessNewEvents();
        }

        private void ThrowIfDisabled()
        {
            if(!settings.EnableDestructiveActions)
                throw new InvalidOperationException("Destructive actions disabled");
        }

        [HttpMethod]
        public void ForgetOldTasks()
        {
            ThrowIfDisabled();
            var ticks = globalTime.GetNowTicks();
            taskIndexController.SetMinTicksHack(ticks);
        }

        private readonly ITaskIndexController taskIndexController;
        private readonly ITaskWriteDynamicSettings settings;
        private readonly IGlobalTime globalTime;
    }
}