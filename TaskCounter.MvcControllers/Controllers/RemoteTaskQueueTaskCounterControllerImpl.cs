using System;

using log4net;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Controllers
{
    public class RemoteTaskQueueTaskCounterControllerImpl
    {
        public RemoteTaskQueueTaskCounterControllerImpl(IRemoteTaskQueueTaskCounterClient remoteTaskQueueTaskCounterClient)
        {
            this.remoteTaskQueueTaskCounterClient = remoteTaskQueueTaskCounterClient;
        }

        public ProcessingTaskCountModel GetProcessingTaskCount()
        {
            int count;
            DateTime time;
            DateTime startTime;
            try
            {
                var c = remoteTaskQueueTaskCounterClient.GetProcessingTaskCount();
                time = new DateTime(c.UpdateTicks, DateTimeKind.Utc);
                startTime = new DateTime(c.StartTicks, DateTimeKind.Utc);
                count = c.Count;
            }
            catch(DomainIsDisabledException e)
            {
                logger.Error("Cannot get TaskCount", e);
                time = jsMinTime;
                startTime = jsMinTime;
                count = 0;
            }
            return new ProcessingTaskCountModel
                {
                    Count = count,
                    UpdateTimeJsTicks = ConvertToJsTicksUtc(time),
                    StartTimeJsTicks = ConvertToJsTicksUtc(startTime),
                };
        }

        private static long ConvertToJsTicksUtc(DateTime time)
        {
            return (long)time.Subtract(jsMinTime).TotalMilliseconds;
        }

        private readonly IRemoteTaskQueueTaskCounterClient remoteTaskQueueTaskCounterClient;
        private static readonly ILog logger = LogManager.GetLogger(typeof(RemoteTaskQueueTaskCounterControllerImpl));
        private static readonly DateTime jsMinTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}