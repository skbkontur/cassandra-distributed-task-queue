using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class DeltaTaskHandler : TaskHandler<DeltaTaskData>
    {
        protected override HandleResult HandleTask(DeltaTaskData taskData)
        {
            return Finish();
        }
    }
}