using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Services;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.TestService
{
    public class TaskCounterServiceEntryPoint : ApplicationBase
    {
        protected override string ConfigFileName => "taskCounterService.csf";

        private static void Main()
        {
            new TaskCounterServiceEntryPoint().Run();
        }

        private void Run()
        {
            Container.ConfigureForTestRemoteTaskQueue();
            Container.Get<HttpService>().Run();
        }
    }
}