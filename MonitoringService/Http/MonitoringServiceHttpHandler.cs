using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Implementation;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Http
{
    public class MonitoringServiceHttpHandler : IHttpHandler
    {
        private readonly IMonitoringServiceImpl monitoringServiceImpl;

        public MonitoringServiceHttpHandler(IMonitoringServiceImpl monitoringServiceImpl)
        {
            this.monitoringServiceImpl = monitoringServiceImpl;
        }

        [HttpMethod]
        public int GetCount()
        {
            return monitoringServiceImpl.GetCount();
        }

        [HttpMethod]
        public TaskMetaInformation[] GetRange(int start, int count)
        {
            return monitoringServiceImpl.GetRange(start, count);
        }

        [HttpMethod]
        public void ActualizeDatabaseScheme()
        {
            monitoringServiceImpl.ActualizeDatabaseScheme();
        }

        [HttpMethod]
        public void DropLocalStorage()
        {
            monitoringServiceImpl.DropLocalStorage();
        }
    }
}