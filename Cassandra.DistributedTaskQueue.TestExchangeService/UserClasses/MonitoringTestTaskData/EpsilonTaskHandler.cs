using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData
{
    public class EpsilonTaskHandler : RtqTaskHandler<EpsilonTaskData>
    {
        protected override HandleResult HandleTask(EpsilonTaskData taskData)
        {
            return Finish();
        }
    }
}