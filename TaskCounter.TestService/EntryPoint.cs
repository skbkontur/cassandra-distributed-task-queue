using RemoteQueue.Settings;

using RemoteTaskQueue.TaskCounter.Http;
using RemoteTaskQueue.TaskCounter.Scheduler;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.TestService
{
    public class EntryPoint : ApplicationBase
    {
        protected override string ConfigFileName { get { return "taskCounterService.csf"; } }

        private static void Main()
        {
            new EntryPoint().Run();
        }

        private void Run()
        {
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            Container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            Container.Configurator.ForAbstraction<IHttpHandler>().UseType<TaskCounterHttpHandler>();
            Container.Get<ITaskCounterServiceSchedulableRunner>().Start();
            Container.Get<HttpService>().Run();
        }
    }
}