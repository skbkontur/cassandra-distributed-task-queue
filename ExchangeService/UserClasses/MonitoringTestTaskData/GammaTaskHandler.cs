using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class GammaTaskHandler : RtqTaskHandler<GammaTaskData>
    {
        protected override HandleResult HandleTask(GammaTaskData taskData)
        {
            return Finish();
        }
    }
}