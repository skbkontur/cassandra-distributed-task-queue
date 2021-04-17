using Microsoft.AspNetCore.Mvc;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
{
    [Route("/[action]")]
    public class MonitoringServiceController : ControllerBase
    {
        public MonitoringServiceController(IMonitoringService monitoringService)
        {
            this.monitoringService = monitoringService;
        }

        [HttpPost]
        public RtqTaskCounters GetTaskCounters()
        {
            return monitoringService.GetTaskCounters();
        }

        [HttpPost]
        public void Stop()
        {
            monitoringService.Stop();
        }

        [HttpPost]
        public void ExecuteForcedFeeding()
        {
            monitoringService.ExecuteForcedFeeding();
        }

        [HttpPost]
        public void ResetState()
        {
            monitoringService.ResetState();
        }

        private readonly IMonitoringService monitoringService;
    }
}