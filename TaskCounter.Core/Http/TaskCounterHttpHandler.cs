using System;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Http
{
    public class TaskCounterHttpHandler : IHttpHandler
    {
        public TaskCounterHttpHandler(ICounterController counterController)
        {
            this.counterController = counterController;
        }

        [HttpMethod]
        public TaskCount GetProcessingTaskCount()
        {
            return counterController.GetTotalCount();
        }

        [HttpMethod]
        public void RestartProcessingTaskCounter(DateTime? fromTime)
        {
            if(fromTime.HasValue)
                counterController.Restart(fromTime.Value.ToUniversalTime().Ticks);
            else
                counterController.Restart(null);
        }

        private readonly ICounterController counterController;
    }
}