using System;

using Newtonsoft.Json;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.OldWaitingTasksCounters;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Http
{
    public class TaskCounterHttpHandler : IHttpHandler
    {
        public TaskCounterHttpHandler(ICounterController counterController, OldWaitingTasksCounter oldWaitingTasksCounter)
        {
            this.counterController = counterController;
            this.oldWaitingTasksCounter = oldWaitingTasksCounter;
        }

        [HttpMethod]
        public TaskCount GetProcessingTaskCount()
        {
            return counterController.GetTotalCount();
        }

        [HttpMethod]
        [JsonHttpMethod]
        public string GetOldWaitingTasksJson()
        {
            return JsonConvert.SerializeObject(oldWaitingTasksCounter.GetOldWaitingTaskIds());
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
        private readonly OldWaitingTasksCounter oldWaitingTasksCounter;
    }
}