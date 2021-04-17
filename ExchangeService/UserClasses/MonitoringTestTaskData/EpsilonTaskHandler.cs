using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class EpsilonTaskHandler : RtqTaskHandler<EpsilonTaskData>
    {
        protected override HandleResult HandleTask(EpsilonTaskData taskData)
        {
            return Finish();
        }
    }
}