using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData
{
    public class DeltaTaskHandler : RtqTaskHandler<DeltaTaskData>
    {
        protected override HandleResult HandleTask(DeltaTaskData taskData)
        {
            return Finish();
        }
    }
}