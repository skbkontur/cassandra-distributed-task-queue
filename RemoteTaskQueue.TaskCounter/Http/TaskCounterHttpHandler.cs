using System;

using Newtonsoft.Json;

using RemoteTaskQueue.TaskCounter.Implementation;
using RemoteTaskQueue.TaskCounter.Implementation.OldWaitingTasksCounters;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace RemoteTaskQueue.TaskCounter.Http
{
    public class TaskCounterHttpHandler : IHttpHandler
    {
        public TaskCounterHttpHandler(CounterController counterController, OldWaitingTasksCounter oldWaitingTasksCounter)
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
            if (fromTime.HasValue)
                counterController.Restart(fromTime.Value.ToUniversalTime().Ticks);
            else
                counterController.Restart(null);
        }

        private readonly CounterController counterController;
        private readonly OldWaitingTasksCounter oldWaitingTasksCounter;
    }
}