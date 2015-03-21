using System;
using System.Web.Mvc;

using log4net;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Client;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Models;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Controllers
{
    public abstract class RemoteTaskQueueTaskCounterControllerBase : ControllerBase
    {
        protected RemoteTaskQueueTaskCounterControllerBase(RemoteTaskQueueTaskCounterControllerParameters parameters)
            : base(parameters.BaseParameters)
        {
            remoteTaskQueueTaskCounterClient = parameters.RemoteTaskQueueTaskCounterClient;
        }

        [HttpGet]
        public ActionResult Run()
        {
            CheckAccess();
            return View("FullScreenTaskCountFast", new TaskCounterModel {GetCountUrl = Url.Action("GetProcessingTaskCount")});
        }

        [HttpGet]
        public JsonResult GetProcessingTaskCount()
        {
            CheckAccess();
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
            return Json(new
                {
                    Count = count,
                    UpdateTimeJsTicks = ConvertToJsTicksUtc(time),
                    StartTimeJsTicks = ConvertToJsTicksUtc(startTime),
                }, JsonRequestBehavior.AllowGet);
        }

        protected abstract void CheckAccess();

/*  TODO Оживить перезапуск, но не путём врезания в мониторинг
        [HttpPost]
        public int RestartCounter(TaskListModelData pageModelData)
        {
            remoteTaskQueueTaskCounterClient.RestartProcessingTaskCounter(DateAndTime.ToDateTime(pageModelData.RestartTime));
            return 1;
        }
*/

        private static long ConvertToJsTicksUtc(DateTime time)
        {
            return (long)time.Subtract(jsMinTime).TotalMilliseconds;
        }

        private readonly IRemoteTaskQueueTaskCounterClient remoteTaskQueueTaskCounterClient;
        private readonly ILog logger = LogManager.GetLogger(typeof(RemoteTaskQueueTaskCounterControllerBase));
        private static readonly DateTime jsMinTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}