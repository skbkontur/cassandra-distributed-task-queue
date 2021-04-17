using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class DeltaTaskHandler : RtqTaskHandler<DeltaTaskData>
    {
        protected override HandleResult HandleTask(DeltaTaskData taskData)
        {
            return Finish();
        }
    }
}