using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData
{
    public class GammaTaskHandler : RtqTaskHandler<GammaTaskData>
    {
        protected override HandleResult HandleTask(GammaTaskData taskData)
        {
            return Finish();
        }
    }
}