using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringService.Http
{
    public class MonitoringServiceHttpHandler : IHttpHandler
    {
        [HttpMethod]
        public void DoNothing()
        {
        }
    }
}