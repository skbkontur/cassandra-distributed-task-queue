using RemoteTaskQueue.TaskCounter.Scheduler;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.TestService
{
    public class TaskCounterServiceHttpHandler : IHttpHandler
    {
        public TaskCounterServiceHttpHandler(TaskCounterServiceSchedulableRunner schedulableRunner)
        {
            this.schedulableRunner = schedulableRunner;
        }

        [HttpMethod]
        public void Start()
        {
            schedulableRunner.Start();
        }

        [HttpMethod]
        public void Stop()
        {
            schedulableRunner.Stop();
        }

        private readonly TaskCounterServiceSchedulableRunner schedulableRunner;
    }
}